namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class HexMap {
        public int Width { get; }
        public int Height { get; }

        

        public float HexSize { get; }
        public List<Hexagon> Hexes { get; }

        
        //private HexMapMeshFlat Mesh { get; set; }
        private HexMapMesh Mesh { get; set; }
        private HexGrid HexGrid { get; set; }


        public bool ShowCoords { get; set; }
        public bool ShowGrid { get; set; }
        public bool Wireframe { get; set; }

        private readonly SpriteFont _font;

        

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

            Rebuild(gd);

        }
        public void Rebuild(GraphicsDevice gd) {
            Mesh = new HexMapMesh(gd, Hexes);
            //Mesh = new HexMapMeshFlat(gd, Hexes);

            HexGrid = new HexGrid(gd, Hexes, Color.Red);
        }

        

        public void Draw(GraphicsDevice gd, BasicEffect effect, SpriteBatch spriteBatch, Camera camera) {
            Mesh.DrawHexes(gd, effect, Wireframe);

            if (ShowGrid) {
                HexGrid.DrawGrid(gd, effect);
            }
            if (ShowCoords) {
                DrawHexCoords(spriteBatch, camera);
            }
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

        

        public Hexagon PickHex(Ray ray) {
            if (ray.Intersects(Mesh.BoundingBox) == null) {
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
            return Mesh.PickVertex(ray);
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