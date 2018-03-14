namespace HexGame {
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    public static class RayExtensions {
        public static float? Intersects(this Ray r, List<Vector3> triangle) {
            var edge1 = triangle[2] - triangle[1];
            var edge2 = triangle[0] - triangle[1];

            var pvec = Vector3.Cross(r.Direction, edge2);

            var det = Vector3.Dot(edge1, pvec);
            if (det > -float.Epsilon && det < float.Epsilon) {
                return null;
            }
            var invDet = 1.0f / det;

            var tvec = r.Position - triangle[1];

            var baryU = Vector3.Dot(tvec, pvec) * invDet;
            if (baryU < 0 || baryU > 1) {
                return null;
            }

            var qvec = Vector3.Cross(tvec, edge1);

            var baryV = Vector3.Dot(r.Direction, qvec)*invDet;
            if (baryV < 0 || baryU + baryV > 1) {
                return null;
            }
            var d = Vector3.Dot(edge2, qvec);
            
            return d * invDet;

        }
    }
}