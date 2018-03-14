namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class HexMap {
        public int Width { get; }
        public int Height { get; }
        public int Count => Width * Height;
        public int TriangleCount => Count * 6;

        public float MaxHeight { get; } = 25;

        public float HexSize { get; }
        public List<List<Hexagon>> Hexes { get; }

        public List<Vector3> Vertices { get; }= new List<Vector3>();
        public List<uint> Indices { get; } = new List<uint>();

        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }

        public VertexBuffer GridVertexBuffer { get; private set; }
        public IndexBuffer GridIndexBuffer { get; private set; }
        private int GridVertexCount { get; set; }
        private int GridIndexCount { get; set; }


        public bool ShowCoords { get; set; }
        public bool ShowGrid { get; set; }

        private readonly SpriteFont _font;

        public HexMap(GraphicsDevice gd, int width, int height, SpriteFont font=null) {
            _font = font;
            HexSize = 0.5f;
            Width = width;
            Height = height;
            Hexes = new List<List<Hexagon>>();

            var hexHeight = Hexagon.Height(HexSize);
            uint i = 0;
            var vertToIndex = new Dictionary<Vector3, uint>();
            for (var x = 0; x < Width; x++) {
                Hexes.Add(new List<Hexagon>());
                for (var y = 0; y < Height; y++) {
                    var position = Vector3.Zero;
                    position.X += 1.5f * HexSize * x;
                    position.Z += hexHeight * y;
                    if (x % 2 == 1) {
                        position.Z += hexHeight / 2;
                    }
                    var hexagon = new Hexagon(position, HexSize) {
                        MapPos = new Vector2(x, y)
                    };
                    Hexes[x].Add(hexagon);
                    foreach (var c in Hexagon.PointOrder) {
                        var p = hexagon.Points[c];
                        if (!vertToIndex.ContainsKey(p)) {
                            vertToIndex[p] = i++;
                            Vertices.Add(p);
                        }
                    }
                    foreach (var c in Hexagon.IndexOrder) {
                        var p = hexagon.Points[c];
                        Indices.Add(vertToIndex[p]);
                    }
                }
            }
            VertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColor), Vertices.Count, BufferUsage.WriteOnly);
            var vpcs = Vertices.Select(v => new VertexPositionColor(v, GetColor(v))).ToArray();
            VertexBuffer.SetData(vpcs);
            IndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());

            BuildGridBuffers(gd);

        }

        private void BuildGridBuffers(GraphicsDevice gd) {
            var verts = new List<VertexPositionColor>();
            var indices = new List<uint>();
            uint i = 0;
            foreach (var row in Hexes) {
                foreach (var hex in row) {
                    var borderVerts = hex.Border;
                    verts.AddRange(borderVerts.Select(v=>new VertexPositionColor(v, Color.Red)));
                    indices.Add(i);
                    indices.Add(i+1);

                    indices.Add(i+1);
                    indices.Add(i+2);

                    indices.Add(i+2);
                    indices.Add(i+3);

                    indices.Add(i+3);
                    indices.Add(i+4);

                    indices.Add(i+4);
                    indices.Add(i+5);

                    indices.Add(i+5);
                    indices.Add(i);

                    i += 6;
                }
            }
            GridIndexCount = indices.Count;
            GridVertexCount = verts.Count;
            GridIndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly );
            GridIndexBuffer.SetData(indices.ToArray());
            GridVertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);
            GridVertexBuffer.SetData(verts.ToArray());
        }

        public void Draw(GraphicsDevice gd, BasicEffect effect, SpriteBatch spriteBatch, Camera camera) {
            DrawHexes(gd, effect);

            if (ShowGrid) {
                DrawGrid(gd, effect);
            }
            if (ShowCoords) {
                DrawHexCoords(spriteBatch, camera);
            }
        }

        private void DrawHexes(GraphicsDevice gd, BasicEffect effect) {
            gd.SetVertexBuffer(VertexBuffer);
            gd.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TriangleCount);
            }
        }

        private void DrawGrid(GraphicsDevice gd, BasicEffect effect) {
            gd.SetVertexBuffer(GridVertexBuffer);
            gd.Indices = GridIndexBuffer;
            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                gd.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, GridIndexCount / 2);
            }
        }

        private void DrawHexCoords(SpriteBatch spriteBatch, Camera camera) {
            if (_font == null) {
                return;
            }
            spriteBatch.Begin();
            foreach (var row in Hexes) {
                foreach (var hex in row) {
                    var projected = spriteBatch.GraphicsDevice.Viewport.Project(hex.Position, camera.ProjectionMatrix, camera.ViewMatrix, camera.WorldMatrix);
                    var screen = new Vector2(projected.X, projected.Y);
                    var pos = $"{hex.MapPos.X}, {hex.MapPos.Y}";
                    var m = _font.MeasureString(pos);
                    spriteBatch.DrawString(_font, pos, screen - m / 2, Color.White);
                }
            }

            spriteBatch.End();
        }

        private Color GetColor(Vector3 v) {
            const float floor = 0.25f;
            const float ceiling = 1.0f;
            var colorStep = ceiling - floor / MaxHeight;
            var value = MathHelper.Clamp(floor + v.Y/MaxHeight * colorStep, floor, ceiling);
            
            
            return new Color(0, value, 0);
        }

        public Hexagon PickHex(Ray ray) {
            var d = float.MaxValue;
            Hexagon ret = null;
            foreach (var row in Hexes) {
                foreach (var hex in row) {
                    var td = hex.IntersectedBy(ray);
                    if (td == null || !(td < d)) {
                        continue;
                    }
                    d = td.Value;
                    ret = hex;

                }
            }
            return ret;
        }
    }
}