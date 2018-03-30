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

        public float MaxHeight { get; } = 5;

        public float HexSize { get; }
        public List<Hexagon> Hexes { get; }

        public List<Vector3> Vertices { get; private set; }
        public List<uint> Indices { get; private set; }

        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }

        private HexGrid HexGrid { get; set; }


        public bool ShowCoords { get; set; }
        public bool ShowGrid { get; set; }
        public bool Wireframe { get; set; }

        private readonly SpriteFont _font;

        public BoundingBox BoundingBox;

        public HexMap(GraphicsDevice gd, int width, int height, SpriteFont font = null) {
            _font = font;
            HexSize = 0.5f;
            Width = width;
            Height = height;
            Hexes = new List<Hexagon>();

            var hexHeight = HexMetrics.Height(HexSize);
            for (var x = 0; x < Width; x++) {
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
                    Hexes.Add(hexagon);
                }
            }

            BuildHexBuffers(gd);

            HexGrid = new HexGrid(gd, Hexes, Color.Red);

        }
        public void Rebuild(GraphicsDevice gd) {
            BuildHexBuffers(gd);

            HexGrid = new HexGrid(gd, Hexes, Color.Red);
        }

        

        private void BuildHexBuffers(GraphicsDevice gd) {
            Vertices = new List<Vector3>();
            Indices = new List<uint>();
            uint i = 0;
            var vertToIndex = new Dictionary<Vector3, uint>(new Vector3Comparer());

            
            foreach (var hexagon in Hexes) {
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
            
            BoundingBox = BoundingBox.CreateFromPoints(Vertices);
            

            VertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColorNormal), Vertices.Count, BufferUsage.WriteOnly);
            var vpcs = Vertices.Select(v => new VertexPositionColorNormal(v, GetColor(v), Vector3.Up)).ToArray();
            GenerateNormals(vpcs, Indices);
            VertexBuffer.SetData(vpcs);
            IndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());
        }

        private void GenerateNormals(VertexPositionColorNormal[] vertices, List<uint> indices) {
            for (var i = 0; i < vertices.Length; i++) {
                vertices[i].Normal = Vector3.UnitY;
            }
            for (var i = 0; i < TriangleCount; i++) {
                var v1 = vertices[indices[i * 3]].Position - vertices[indices[i * 3 + 1]].Position;
                var v2 = vertices[indices[i * 3 + 2]].Position - vertices[indices[i * 3+1]].Position;
                var normal = Vector3.Cross(v1, v2);
                normal.Normalize();
                vertices[indices[i * 3]].Normal += normal;
                vertices[indices[i * 3 + 1]].Normal += normal;
                vertices[indices[i * 3 + 2]].Normal += normal;
            }
            for (var i = 0; i < vertices.Length; i++) {
                vertices[i].Normal.Normalize();
            }
        }

        

        public void Draw(GraphicsDevice gd, BasicEffect effect, SpriteBatch spriteBatch, Camera camera) {
            DrawHexes(gd, effect);

            if (ShowGrid) {
                HexGrid.DrawGrid(gd, effect);
            }
            if (ShowCoords) {
                DrawHexCoords(spriteBatch, camera);
            }
        }

        private void DrawHexes(GraphicsDevice gd, BasicEffect effect) {
            gd.SetVertexBuffer(VertexBuffer);
            gd.Indices = IndexBuffer;
            var rs = gd.RasterizerState;
            if (Wireframe) {
                gd.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };
            }


            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1, -1, 0));
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);

            effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);

            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TriangleCount);
            }
            gd.RasterizerState = rs;
        }

        

        private void DrawHexCoords(SpriteBatch spriteBatch, Camera camera) {
            if (_font == null) {
                return;
            }
            spriteBatch.Begin();
            foreach (var hex in Hexes) {
                var projected = spriteBatch.GraphicsDevice.Viewport.Project(hex.Position, camera.ProjectionMatrix, camera.ViewMatrix, camera.WorldMatrix);
                var screen = new Vector2(projected.X, projected.Y);
                var pos = $"{hex.MapPos.X}, {hex.MapPos.Y}";
                var m = _font.MeasureString(pos);
                spriteBatch.DrawString(_font, pos, screen - m / 2, Color.White);
            }
            

            spriteBatch.End();
        }

        private Color GetColor(Vector3 v) {
            const float floor = 0.25f;
            const float ceiling = 1.0f;
            const float median = (floor + ceiling) / 2;
            var colorStep = ceiling - floor / (MaxHeight * 2);
            var value = MathHelper.Clamp(median + v.Y / MaxHeight * colorStep, floor, ceiling);


            return new Color(0, value, 0);
        }

        public Hexagon PickHex(Ray ray) {
            if (ray.Intersects(BoundingBox) == null) {
                return null;
            }
            var d = float.MaxValue;
            Hexagon ret = null;
            foreach (var hex in Hexes) {
                var td = hex.IntersectedBy(ray);
                if (td == null || !(td < d)) {
                    continue;
                }
                d = td.Value;
                ret = hex;

            }
            
            return ret;
        }

        public Vector3? PickVertex(Ray ray) {
            if (ray.Intersects(BoundingBox) == null) {
                return null;
            }
            var d = float.MaxValue;
            Vector3? ret = null;
            var sphere = new BoundingSphere(Vector3.Zero, HexSize/3);
            foreach (var vertex in Vertices) {
                var td = ray.Intersects(sphere.Transform(Matrix.CreateTranslation(vertex)));
                if (td == null || !(td < d)) {
                    continue;
                }
                d = td.Value;
                ret = vertex;
            }

            return ret;
        }

        public void RaiseVertex(Vector3 vertex, float dy) {
            var comparer = new Vector3Comparer();
            var affectedHexes = Hexes.Where(h => h.Points.Values.Any(v => comparer.Equals(v, vertex)))
                                     .Select(h => new {
                                         hex = h, 
                                         point = h.Points.First(p=>comparer.Equals(p.Value, vertex)).Key
                                      });
            foreach (var affectedHex in affectedHexes) {
                affectedHex.hex.Raise(dy, affectedHex.point);
            }
        }
    }
}