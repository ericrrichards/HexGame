namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Xna.Framework;

    using NUnit.Framework;

    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    public class Hexagon {
        internal string DebugDisplayString => $"{MapPos.X} {MapPos.Y}";
        public float HexWidth { get; }
        public Vector3 Position { get; }
        public Point MapPos { get; set; }

        public Dictionary<HexagonPoint, Vector3> Points { get; }
        public BoundingBox BoundingBox { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public List<Vector3> Border { get; private set; }
        public Dictionary<HexDirection, Hexagon> Neighbors { get; } = new Dictionary<HexDirection, Hexagon>();

        public Hexagon(Vector3 position, float hexWidth = 1.0f) {
            HexWidth = hexWidth;
            Position = position;

            Points = HexMetrics.PointOrder.ToDictionary(p => p, p => HexMetrics.GetPoint(p, Position, HexWidth));
            BuildBounds();
        }

        private void BuildBounds() {
            BoundingBox = BoundingBox.CreateFromPoints(Points.Values);
            Triangles = new List<Triangle>();
            var indices = HexMetrics.IndexOrder.ToList();
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
        public void Raise(float dy, IEnumerable<HexagonPoint> points) {
            foreach (var p in points) {
                var center=  Points[p];
                center.Y += dy;
                Points[p] = center;
            }
            BuildBounds();
        }

        public bool CanRaisePoint(HexagonPoint p, float dy) {
            var v = Points[p];
            var newHeight = v.Y + dy;
            var triangles = Triangles.Where(t => t.Points.Any(tp => tp == v));

            foreach (var triangle in triangles) {
                var otherPoints = triangle.Points.Except(new[] { v });
                if (otherPoints.Any(otherPoint => Math.Abs(newHeight - otherPoint.Y) > Math.Abs(dy))) {
                    return false;
                }
            }
            return true;
        }


        public List<HexagonPoint> GetMatchingPoints(Hexagon neighbor) {
            var comparer = new Vector3Comparer();
            var points = Border.ToList();
            return neighbor.Points.Where(np => points.Any(p => comparer.Equals(p, np.Value))).Select(np => np.Key).ToList();
        }
    }
    [TestFixture]
    public class HexagonTests {
        [TestCase(0, 0, HexDirection.SouthEast, new[]{HexagonPoint.TopLeft, HexagonPoint.Left})]
        [TestCase(0, 0, HexDirection.South, new[]{HexagonPoint.TopLeft, HexagonPoint.TopRight})]

        [TestCase(0, 1, HexDirection.North, new[]{HexagonPoint.BottomLeft, HexagonPoint.BottomRight})]
        [TestCase(0, 1, HexDirection.South, new[]{HexagonPoint.TopLeft, HexagonPoint.TopRight})]
        [TestCase(0, 1, HexDirection.NorthEast, new[]{HexagonPoint.Left, HexagonPoint.BottomLeft})]
        [TestCase(0, 1, HexDirection.SouthEast, new[]{HexagonPoint.TopLeft, HexagonPoint.Left})]


        [TestCase(1, 0, HexDirection.NorthWest, new[]{HexagonPoint.Right, HexagonPoint.BottomRight})]
        [TestCase(1, 0, HexDirection.SouthWest, new[]{HexagonPoint.TopRight, HexagonPoint.Right})]
        [TestCase(1, 0, HexDirection.South, new[]{HexagonPoint.TopLeft, HexagonPoint.TopRight})]
        [TestCase(1, 0, HexDirection.SouthEast, new[]{HexagonPoint.TopLeft, HexagonPoint.Left})]
        [TestCase(1, 0, HexDirection.NorthEast, new[]{HexagonPoint.Left, HexagonPoint.BottomLeft})]

        public void GetMatchingPoints(int x, int y, HexDirection dir, HexagonPoint[] expected) {
            var hex = new Hexagon(Position(x,y));

            var offset = HexMetrics.GetNeighborCoords(new Point(x,y), dir);

            var neighbor = new Hexagon(Position(offset.X, offset.Y));

            var ret = hex.GetMatchingPoints(neighbor);
            Assert.True(ret.Count == expected.Length, $"expected has {expected.Length} actual has {ret.Count}");
            foreach (var hexagonPoint in expected) {
                Assert.True(ret.Contains(hexagonPoint), hexagonPoint + " is missing!");
            }

        }

        private static Vector3 Position(int x, int y) {
            var hexSize = 1.0f;
            var hexHeight = HexMetrics.Height(hexSize);
            var position = Vector3.Zero;
            position.X += 1.5f * hexSize * x;
            position.Z += hexHeight * y;
            if (x % 2 == 1) {
                position.Z += hexHeight / 2;
            }
            return position;
        }
    }
}