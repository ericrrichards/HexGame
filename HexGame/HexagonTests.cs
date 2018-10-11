namespace HexGame {
    using Microsoft.Xna.Framework;

    using NUnit.Framework;

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
            var hex = new Hexagon(new Point(x,y));

            var offset = HexMetrics.GetNeighborCoords(new Point(x,y), dir);

            var neighbor = new Hexagon(offset);

            var ret = hex.GetMatchingPoints(neighbor);
            Assert.True(ret.Count == expected.Length, $"expected has {expected.Length} actual has {ret.Count}");
            foreach (var hexagonPoint in expected) {
                Assert.True(ret.Contains(hexagonPoint), hexagonPoint + " is missing!");
            }

        }
    }
}