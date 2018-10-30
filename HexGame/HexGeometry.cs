namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;

    public class HexGeometry {
        public float HeightStep => HexWidth/2;
        public float HexWidth { get; }
        public Vector3 Position { get; private set; }
        public Dictionary<HexagonPoint, Vector3> Points { get; }
        public BoundingBox BoundingBox { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public List<Vector3> Border => Points.Where(kv => kv.Key != HexagonPoint.Center).Select(kv => kv.Value).ToList();

        public HexGeometry(Point mapPos, float hexWidth = 1.0f) {
            HexWidth = hexWidth;
            Position = GetHexCenter(mapPos);
            Points = HexMetrics.PointOrder.ToDictionary(p => p, p => HexMetrics.GetPoint(p, Position, HexWidth));
            BuildBounds();
        }
        private Vector3 GetHexCenter(Point pos) {
            var hexHeight = HexMetrics.Height(HexWidth);
            var position = Vector3.Zero;
            position.X += 1.5f * HexWidth * pos.X;
            position.Z += hexHeight * pos.Y;
            if (pos.X % 2 == 1) {
                position.Z += hexHeight / 2;
            }
            return position;
        }

        internal void AdjustHeights(int[] heights) {
            for (var i = 0; i < heights.Length; i++) {
                var point = Points[(HexagonPoint)i];
                point.Y = heights[i] * HeightStep;
                Points[(HexagonPoint)i] = point;
            }
            Position = Points[HexagonPoint.Center];
            BuildBounds();
        }
        public List<Vector3> GetMidPoints() {
            return Border.Select(p => Vector3.Lerp(p, Position, 0.5f)).Union(new[] { Position }).ToList();
        }
        public float? IntersectedBy(Ray ray) {
            var d = float.MaxValue;
            var td = ray.Intersects(BoundingBox);
            if (td == null) {
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

        private void BuildBounds() {
            BoundingBox = BoundingBox.CreateFromPoints(Points.Values);
            Triangles = new List<Triangle>();
            var indices = HexMetrics.IndexOrder.ToList();
            while (indices.Any()) {
                var tri = indices.Take(3).Select(i => Points[i]).ToList();
                var uvs = indices.Take(3).Select(i => HexMetrics.UVs[i]).ToList();

                Triangles.Add(new Triangle(tri, uvs));
                indices = indices.Skip(3).ToList();
            }
        }
        public bool CanRaisePoint(HexagonPoint p, int amount) {
            var dy = HeightStep * amount;
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
        public void Raise(int dy, HexagonPoint p = HexagonPoint.Center) {
            var point = Points[p];
            point.Y += dy * HeightStep;
            Points[p] = point;
            if (p == HexagonPoint.Center) {
                Position = point;
            }
            BuildBounds();
        }
    }
}