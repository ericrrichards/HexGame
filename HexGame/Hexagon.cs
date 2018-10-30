namespace HexGame {
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Xna.Framework;

    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    public class Hexagon {
        private string DebugDisplayString => $"{MapPos.X} {MapPos.Y}";
        public HexGeometry Geometry { get; }
        
        
        public Point MapPos { get; }
        public int Height { get; private set; }

        
        public int PatchID { get; set; } = -1;

        public bool IsForest { get; set; }
        public Dictionary<HexDirection, Hexagon> Neighbors { get; } = new Dictionary<HexDirection, Hexagon>();

        public Hexagon(Point mapPosition, float hexWidth = 1.0f) {
            MapPos = mapPosition;
            Geometry = new HexGeometry(mapPosition, hexWidth);
            Height = 0;
        }

        public Hexagon(HexRecord hexRecord, float hexWidth = 1.0f) {
            MapPos = hexRecord.MapPos;
            Geometry = new HexGeometry(hexRecord.MapPos, hexWidth);
            Height = hexRecord.Heights[(int)HexagonPoint.Center];
            Geometry.AdjustHeights(hexRecord.Heights);
            
            IsForest = hexRecord.Forested;
            
        }
        
        public List<HexagonPoint> GetMatchingPoints(Hexagon neighbor) {
            var comparer = new Vector3Comparer();
            var points = Geometry.Border.ToList();
            return neighbor.Geometry.Points.Where(np => points.Any(p => comparer.Equals(p, np.Value))).Select(np => np.Key).ToList();
        }

        public void Raise(HexagonPoint point) {
            if (point == HexagonPoint.Center) {
                Height++;
            }
            Geometry.Raise(1, point);
        }

        public void Lower(HexagonPoint point) {
            if (point == HexagonPoint.Center) {
                Height--;
            }
            Geometry.Raise(-1, point);
        }

        public bool CanRaisePoint(HexagonPoint point, int amount) {
            return Geometry.CanRaisePoint(point, amount);
        }
    }
}