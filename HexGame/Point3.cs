namespace HexGame {
    using System;
    using System.Diagnostics;

    using Microsoft.Xna.Framework;

    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    public struct Point3 : IEquatable<Point3> {
        private static readonly Point3 ZeroPoint = new Point3();

        public int X;
        public int Y;
        public int Z;

        public static Point3 Zero => ZeroPoint;
        internal string DebugDisplayString => $"{X} {Y} {Z}";

        public Point3(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(int value) {
            X = value;
            Y = value;
            Z = value;
        }

        public static Point3 operator +(Point3 l, Point3 r) {
            return new Point3(l.X+r.X, l.Y+r.Y, l.Z +r.Z);
        }

        public static Point3 operator -(Point3 l, Point3 r) {
            return new Point3(l.X-r.X, l.Y-r.Y, l.Z-r.Z);
        }

        public static Point3 operator *(Point3 l, Point3 r) {
            return new Point3(l.X*r.X, l.Y*r.Y, l.Z*r.Z);
        }
        public static Point3 operator /(Point3 l, Point3 r) {
            return new Point3(l.X/r.X, l.Y/r.Y, l.Z/r.Z);
        }

        public static bool operator ==(Point3 l, Point3 r) {
            return l.Equals(r);
        }

        public static bool operator !=(Point3 l, Point3 r) {
            return !l.Equals(r);
        }

        public override bool Equals(object obj) {
            if (obj is Point3 point3) {
                return Equals(point3);
            }
            return false;
        }

        public bool Equals(Point3 other) {
            if (X == other.X) {
                if (Y == other.Y) {
                    return Z == other.Z;
                }
            }
            return false;
        }

        public override int GetHashCode() {
            return (17 * 23 * 19 + X.GetHashCode()) * 23 *19 + Y.GetHashCode() + 19*Z.GetHashCode();
        }

        public override string ToString() {
            return $"{{X:{X} Y:{Y} Z:{Z}}}";
        }

        public Vector3 ToVector3() {
            return new Vector3(X,Y,Z);
        }
    }
}