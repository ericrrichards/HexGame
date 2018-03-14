namespace HexGame {
    using System.Runtime.InteropServices;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColorNormal : IVertexType {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            
        );


        public Vector3 Position;
        public Vector3 Normal;
        public Color Color;

        public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal) {
            Position = position;
            Color = color;
            Normal = normal;
        }
        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
        public override int GetHashCode() {
            return 0;
        }
        public override string ToString() {
            return $"{{{{Position:{Position} Color:{Color} Normal:{Normal}}}}}";
        }
        public static bool operator ==(VertexPositionColorNormal left, VertexPositionColorNormal right) {
            if (left.Normal == right.Normal) {
                if (left.Color == right.Color) {
                    return left.Position == right.Position;
                }
            }
            return false;
        }
        public static bool operator !=(VertexPositionColorNormal left, VertexPositionColorNormal right) {
            return !(left == right);
        }
        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != GetType()) {
                return false;
            }
            return this == (VertexPositionColorNormal)obj;
        }
    }
}