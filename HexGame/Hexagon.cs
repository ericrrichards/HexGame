namespace HexGame {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.Xna.Framework;

    public class Hexagon {
        public static readonly IReadOnlyList<HexagonPoint> PointOrder = new ReadOnlyCollection<HexagonPoint>(new List<HexagonPoint> {
            HexagonPoint.Center, HexagonPoint.TopLeft, HexagonPoint.TopRight, HexagonPoint.Right, 
            HexagonPoint.BottomRight, HexagonPoint.BottomLeft, HexagonPoint.Left
        });
        public static readonly IReadOnlyList<HexagonPoint> IndexOrder = new ReadOnlyCollection<HexagonPoint>(new List<HexagonPoint> {
            HexagonPoint.Center, HexagonPoint.TopLeft, HexagonPoint.TopRight,
            HexagonPoint.Center, HexagonPoint.TopRight, HexagonPoint.Right,
            HexagonPoint.Center, HexagonPoint.Right, HexagonPoint.BottomRight,
            HexagonPoint.Center, HexagonPoint.BottomRight, HexagonPoint.BottomLeft,
            HexagonPoint.Center, HexagonPoint.BottomLeft, HexagonPoint.Left,
            HexagonPoint.Center, HexagonPoint.Left, HexagonPoint.TopLeft
        });

        public float HexWidth { get; }
        public Vector3 Position { get; }
        public Vector2 MapPos { get; set; }

        public Dictionary<HexagonPoint, Vector3> Points { get; }
        public BoundingBox BoundingBox { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public List<Vector3> Border { get; private set; }

        public Hexagon(Vector3 position, float hexWidth = 1.0f) {
            HexWidth = hexWidth;
            Position = position;

            Points = PointOrder.ToDictionary(p => p, p => HexMetrics.GetPoint(p, Position, HexWidth));
            BuildBounds();
        }

        private void BuildBounds() {
            BoundingBox = BoundingBox.CreateFromPoints(Points.Values);
            Triangles = new List<Triangle>();
            var indices = IndexOrder.ToList();
            while (indices.Any()) {
                var tri = indices.Take(3).Select(i => Points[i]).ToList();
                Triangles.Add(new Triangle(tri));
                indices = indices.Skip(3).ToList();
            }
            Border = Points.Where(kv => kv.Key != HexagonPoint.Center).Select(kv => kv.Value).ToList();
        }

        public float? IntersectedBy(Ray ray) {
            var d = float.MaxValue;
            var td = ray.Intersects(BoundingBox);
            if (td == null ) {
                return null;
            }
            foreach (var tri in Triangles) {         
                td = ray.Intersects(tri);
                if (td == null || !(td < d)) {
                    continue;
                }
                d = td.Value;
                
            }
            return d;
        }

        public void Raise(float dy, HexagonPoint p=HexagonPoint.Center) {
            var center=  Points[p];
            center.Y += dy;
            Points[p] = center;
            BuildBounds();
        }
    }
}