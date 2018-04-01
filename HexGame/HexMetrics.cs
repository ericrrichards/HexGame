namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Microsoft.Xna.Framework;

    using NUnit.Framework;

    public static class HexMetrics {

        public static readonly IReadOnlyList<HexagonPoint> PointOrder = new ReadOnlyCollection<HexagonPoint>(new List<HexagonPoint> {
            HexagonPoint.Center, HexagonPoint.TopLeft, HexagonPoint.TopRight, HexagonPoint.Right, 
            HexagonPoint.BottomRight, HexagonPoint.BottomLeft, HexagonPoint.Left
        });
        public static readonly Dictionary<HexagonPoint, Vector2> UVs = new Dictionary<HexagonPoint, Vector2> {
            [HexagonPoint.Center] = new Vector2(0.5f, 0.5f),
            [HexagonPoint.Left] = new Vector2(0, 0.5f),
            [HexagonPoint.Right] = new Vector2(1, 0.5f),
            [HexagonPoint.TopLeft] = new Vector2(.25f, 0),
            [HexagonPoint.TopRight] = new Vector2(.75f, 0),
            [HexagonPoint.BottomLeft] = new Vector2(.25f, 1),
            [HexagonPoint.BottomRight] = new Vector2(.75f, 1)
        };

        public static readonly IReadOnlyList<HexagonPoint> IndexOrder = new ReadOnlyCollection<HexagonPoint>(new List<HexagonPoint> {
            HexagonPoint.Center, HexagonPoint.TopLeft, HexagonPoint.TopRight,
            HexagonPoint.Center, HexagonPoint.TopRight, HexagonPoint.Right,
            HexagonPoint.Center, HexagonPoint.Right, HexagonPoint.BottomRight,
            HexagonPoint.Center, HexagonPoint.BottomRight, HexagonPoint.BottomLeft,
            HexagonPoint.Center, HexagonPoint.BottomLeft, HexagonPoint.Left,
            HexagonPoint.Center, HexagonPoint.Left, HexagonPoint.TopLeft
        });

        public static Vector3 GetPoint(HexagonPoint p, Vector3 center, float hexWidth) {
            var y = HalfHeight(hexWidth);
            var x = HalfWidth(hexWidth);
            switch (p) {
                case HexagonPoint.Center:
                    return center;
                case HexagonPoint.TopLeft:
                    return new Vector3(-x, 0, -y) + center;
                case HexagonPoint.TopRight:
                    return new Vector3(x, 0, -y) + center;
                case HexagonPoint.Right:
                    return new Vector3(hexWidth, 0, 0) + center;
                case HexagonPoint.BottomRight:
                    return new Vector3(x, 0, y) + center;
                case HexagonPoint.BottomLeft:
                    return new Vector3(-x, 0, y) + center;
                case HexagonPoint.Left:
                    return new Vector3(-hexWidth, 0, 0) + center;
                default:
                    throw new ArgumentOutOfRangeException(nameof(p), p, null);
            }
        }

        public static float HalfHeight(float hexWidth) {
            var y = (float)(hexWidth * Math.Sin(MathHelper.ToRadians(60)));
            return y;
        }

        public static float Height(float hexWidth) {
            return 2 * HalfHeight(hexWidth);
        }

        public static float HalfWidth(float hexSize) {
            return hexSize / 2;
        }


        public static Point CubicToOffset(Point3 cube) {
            var col = cube.X;
            var row = cube.Z + (cube.X - (cube.X & 1)) / 2;
            return new Point(col, row);
        }

        public static Point3 OffsetToCubic(Point p) {
            var x = p.X;
            var z = p.Y - (p.Y - (p.Y & 1)) / 2;
            var y = -x - z;
            return new Point3(x, y, z);
        }

        public static readonly Point[][] DirectionOffsets = {
            new[] { new Point(0, -1), new Point(1, -1), new Point(1,0),new Point(0, 1),new Point(-1, 0), new Point(-1, -1) },
            new[] { new Point(0, -1), new Point(1, 0), new Point(1,1),new Point(0, 1),new Point(-1, 1), new Point(-1, 0) }
        };

        public static Point GetNeighborCoords(Point hex, HexDirection dir) {
            var parity = hex.X & 1;
            var offset = DirectionOffsets[parity][(int)dir];
            return new Point(hex.X + offset.X, hex.Y + offset.Y);
        }
    }
    [TestFixture]
    public class HexMetricsTests {


        [TestCase(0,0, HexDirection.North, 0, -1)]
        [TestCase(0,0, HexDirection.NorthEast, 1, -1)]
        [TestCase(0,0, HexDirection.SouthEast, 1, 0)]
        [TestCase(0,0, HexDirection.South, 0, 1)]
        [TestCase(0,0, HexDirection.SouthWest, -1, 0)]
        [TestCase(0,0, HexDirection.NorthWest, -1, -1)]
        [TestCase(0,1, HexDirection.North, 0, 0)]
        [TestCase(0,1, HexDirection.NorthEast, 1, 0)]
        [TestCase(0,1, HexDirection.SouthEast, 1, 1)]
        [TestCase(0,1, HexDirection.South, 0, 2)]
        [TestCase(0,1, HexDirection.SouthWest, -1, 1)]
        [TestCase(0,1, HexDirection.NorthWest, -1, 0)]
        [TestCase(1,0, HexDirection.North, 1, -1)]
        [TestCase(1,0, HexDirection.NorthEast, 2, 0)]
        [TestCase(1,0, HexDirection.SouthEast, 2, 1)]
        [TestCase(1,0, HexDirection.South, 1, 1)]
        [TestCase(1,0, HexDirection.SouthWest, 0, 1)]
        [TestCase(1,0, HexDirection.NorthWest, 0, 0)]
        public void GetNeighborCoords(int hx, int hy, HexDirection d, int ex, int ey) {
            Assert.AreEqual(new Point(ex,ey), HexMetrics.GetNeighborCoords(new Point(hx, hy), d));
        }
    }
}