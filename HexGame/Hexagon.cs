namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.Xna.Framework;

    public enum HexagonPoint {
        Center = 0,
        TopLeft = 1,
        TopRight = 2,
        Right = 3,
        BottomRight = 4,
        BottomLeft = 5,
        Left = 6
    }

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

        public float Size { get; }
        public Vector3 Position { get; }
        public Vector2 MapPos { get; set; }

        public Dictionary<HexagonPoint, Vector3> Points { get; }
        public BoundingBox BoundingBox { get; private set; }
        public List<List<Vector3>> Triangles { get; private set; }
        public List<Vector3> Border { get; private set; }

        public Hexagon(Vector3 position, float size = 1.0f) {
            Size = size;
            Position = position;

            Points = PointOrder.ToDictionary(p => p, p => GetPoint(p, Position, Size));
            BuildBounds();
        }

        private void BuildBounds() {
            BoundingBox = BoundingBox.CreateFromPoints(Points.Values);
            Triangles = new List<List<Vector3>>();
            var indices = IndexOrder.ToList();
            while (indices.Any()) {
                var tri = indices.Take(3).Select(i => Points[i]).ToList();
                Triangles.Add(tri);
                indices = indices.Skip(3).ToList();
            }
            Border = Points.Where(kv => kv.Key != HexagonPoint.Center).Select(kv => kv.Value).ToList();
        }

        public static Vector3 GetPoint(HexagonPoint p, Vector3 center, float size) {
            var y = HalfHeight(size);
            var x = HalfWidth(size);
            switch (p) {
                case HexagonPoint.Center:
                    return center;
                case HexagonPoint.TopLeft:
                    return new Vector3(-x, 0, -y) + center;
                case HexagonPoint.TopRight:
                    return new Vector3(x, 0, -y) + center;
                case HexagonPoint.Right:
                    return new Vector3(size, 0, 0) + center;
                case HexagonPoint.BottomRight:
                    return new Vector3(x, 0, y) + center;
                case HexagonPoint.BottomLeft:
                    return new Vector3(-x, 0, y) + center;
                case HexagonPoint.Left:
                    return new Vector3(-size, 0, 0) + center;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p), p, null);
            }
        }

        public static float HalfHeight(float size) {
            var y = (float)(size * Math.Sin(MathHelper.ToRadians(60)));
            return y;
        }

        public static float Height(float size) {
            return 2 * HalfHeight(size);
        }

        public static float HalfWidth(float size) {
            return size / 2;
        }

        public static float Width(float size) {
            return 2 * HalfWidth(size);
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

    }
}