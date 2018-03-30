namespace HexGame {
    using System;
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public class Vector3Comparer : IEqualityComparer<Vector3> {
        private float Tolerance { get; }

        public Vector3Comparer(float tolerance= 0.0001f) {
            Tolerance = tolerance;
        }

        public bool Equals(Vector3 x, Vector3 y) {
            return Math.Abs(x.X - y.X) < Tolerance && Math.Abs(x.Y - y.Y) < Tolerance && Math.Abs(x.Z - y.Z) < Tolerance;
        }

        public int GetHashCode(Vector3 obj) {
            return obj.GetHashCode();
        }
    }
}