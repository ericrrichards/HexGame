﻿namespace HexGame {
    using System;

    using Microsoft.Xna.Framework;

    public static class HexMetrics {
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
    }
}