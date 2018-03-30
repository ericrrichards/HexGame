namespace HexGame {
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public class Triangle {
        public Vector3 P0 => Points[0];
        public Vector3 P1 => Points[1];
        public Vector3 P2 => Points[2];
        public List<Vector3> Points { get; }

        public Triangle(List<Vector3> ps) {
            Points = ps;
        }
    }
}