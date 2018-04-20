namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Xna.Framework;

    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    public class Hexagon {
        internal string DebugDisplayString => $"{MapPos.X} {MapPos.Y}";
        public float HexWidth { get; }
        public Vector3 Position { get; private set; }
        public Point MapPos { get; set; }

        public Dictionary<HexagonPoint, Vector3> Points { get; }
        public BoundingBox BoundingBox { get; private set; }
        public List<Triangle> Triangles { get; private set; }
        public List<Vector3> Border { get; private set; }
        public Dictionary<HexDirection, Hexagon> Neighbors { get; } = new Dictionary<HexDirection, Hexagon>();
        public int PatchID { get; set; }

        public bool IsForest { get; set; }

        public Hexagon(Vector3 position, float hexWidth = 1.0f) {
            PatchID = -1;
            HexWidth = hexWidth;
            Position = position;

            Points = HexMetrics.PointOrder.ToDictionary(p => p, p => HexMetrics.GetPoint(p, Position, HexWidth));

            BuildBounds();
        }

        public Hexagon(Vector3 position, float hexWidth, HexRecord hexRecord, float heightStep) {
            PatchID = -1;
            HexWidth = hexWidth;
            Position = position;

            Points = HexMetrics.PointOrder.ToDictionary(p => p, p => HexMetrics.GetPoint(p, Position, HexWidth));
            for (var i = 0; i < hexRecord.Heights.Length; i++) {
                var point = Points[(HexagonPoint)i];
                point.Y = hexRecord.Heights[i] * heightStep;
                Points[(HexagonPoint)i] = point;
            }
            Position = Points[HexagonPoint.Center];
            IsForest = hexRecord.Forested;
            BuildBounds();
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
            if (p == HexagonPoint.Center) {
                Position = center;
            }
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

        public List<Vector3> GetMidPoints() {
            return Border.Select(p => Vector3.Lerp(p, Position, 0.5f)).ToList();
        }
    }
}