namespace HexGame {
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public class Triangle {
        public Vector3 P0 => Points[0];
        public Vector3 P1 => Points[1];
        public Vector3 P2 => Points[2];
        public List<Vector3> Points { get; }

        public List<Vector2> UVs { get; }
        public Vector2 UV0 => UVs[0];
        public Vector2 UV1 => UVs[1];
        public Vector2 UV2 => UVs[2];

        public Triangle(List<Vector3> ps, List<Vector2> uvs) {
            Points = ps;
            UVs = uvs;
        }
    }
}