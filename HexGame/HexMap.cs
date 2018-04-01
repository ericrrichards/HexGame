namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public enum MeshType {
        Smooth,
        Flat
    }

    public class HexMap {
        public int Width { get; }
        public int Height { get; }

        

        public float HexSize { get; }
        public List<Hexagon> Hexes { get; }

        
        
        private HexMapMesh Mesh { get; set; }
        private HexGrid HexGrid { get; set; }


        public bool ShowCoords { get; set; }
        public bool ShowGrid { get; set; }
        public bool Wireframe { get; set; }
        public MeshType MeshType { get; }

        private readonly SpriteFont _font;

        private const float HeightStep = 0.25f;
        

        public HexMap(GraphicsDevice gd, int width, int height, SpriteFont font = null, MeshType meshType=MeshType.Smooth) {
            _font = font;
            HexSize = 0.5f;
            Width = width;
            Height = height;
            Hexes = new List<Hexagon>();
            MeshType = meshType;

            var hexHeight = HexMetrics.Height(HexSize);
            for (var x = 0; x < Width; x++) {
                for (var y = 0; y < Height; y++) {
                    //TODO bump this out into a helper function
                    var position = Vector3.Zero;
                    position.X += 1.5f * HexSize * x;
                    position.Z += hexHeight * y;
                    if (x % 2 == 1) {
                        position.Z += hexHeight / 2;
                    }
                    var hexagon = new Hexagon(position, HexSize) {
                        MapPos = new Point(x, y)
                    };
                    Hexes.Add(hexagon);
                }
            }
            foreach (var hexagon in Hexes) {
                foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) {
                    hexagon.Neighbors[dir] = GetHex(HexMetrics.GetNeighborCoords(hexagon.MapPos, dir));
                }
                
            }

            Rebuild(gd);

        }
        public void Rebuild(GraphicsDevice gd) {
            switch (MeshType) {
                case MeshType.Smooth:
                    Mesh = new HexMapMeshSmooth(gd, Hexes);
                    break;
                case MeshType.Flat:
                    Mesh = new HexMapMeshFlat(gd, Hexes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            HexGrid = new HexGrid(gd, Hexes, Color.Gray);
        }
        [CanBeNull]
        public Hexagon GetHex(Point p) {
            return GetHex(p.X, p.Y);
        }
        [CanBeNull]
        public Hexagon GetHex(int x, int y) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) {
                return null;
            }
            return Hexes[y + x * Width];
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
        
        private void RaiseVertex(Vector3 vertex, float dy) {
            // TODO limit raising vertices to only allow one height step above/below surrounding vertices

            var comparer = new Vector3Comparer();
            var affectedHexes = Hexes.Where(h => h.Points.Values.Any(v => comparer.Equals(v, vertex)))
                                     .Select(h => new {
                                         hex = h, 
                                         point = h.Points.First(p=>comparer.Equals(p.Value, vertex)).Key
                                      }).ToList();
            if (!affectedHexes.All(h => h.hex.CanRaisePoint(h.point, dy))) {
                return;
            }
            foreach (var affectedHex in affectedHexes) {
                affectedHex.hex.Raise(dy, affectedHex.point);
            }
        }

        public void RaiseVertex(Vector3 vertex) {
            RaiseVertex(vertex, HeightStep);
        }

        public void LowerVertex(Vector3 vertex) {
            RaiseVertex(vertex, -HeightStep);
        }

        private void RaiseHex(Hexagon hex, float dy) {
            var neighbors = hex.Neighbors.Values.Where(n=>n!=null).ToList();
            foreach (var neighbor in neighbors) {
                var pointsToRaise = hex.GetMatchingPoints(neighbor);
                neighbor.Raise(dy, pointsToRaise);
            }
            hex.Raise(dy, HexMetrics.PointOrder);
        }

        public void RaiseHex(Hexagon hex) {
            RaiseHex(hex, HeightStep);
        }

        public void LowerHex(Hexagon hex) {
            RaiseHex(hex, -HeightStep);
        }
    }
}