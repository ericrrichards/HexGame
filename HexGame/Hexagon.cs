namespace HexGame {
    using System;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Hexagon {
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }

        private VertexPositionColor[] TriangleVertices { get; }
        private static readonly uint[] HexagonIndices = {
            0,1,2,
            0,2,3,
            0,3,4,
            0,4,5,
            0,5,6,
            0,6,1
        };
        private static VertexPositionColor[] MakeHexagonVertices(Vector3 position, float size) {
            var y = (float)(size * Math.Sin(MathHelper.ToRadians(60)));
            var x = size / 2;

            var center = new VertexPositionColor(position, Color.White);
            var topLeft = new VertexPositionColor(new Vector3(-x, y, 0), Color.Yellow);
            var topRight = new VertexPositionColor(new Vector3(x, y, 0), Color.Green);
            var right = new VertexPositionColor(new Vector3(size, 0, 0), Color.Cyan);
            var bottomRight = new VertexPositionColor(new Vector3(x, -y, 0), Color.Blue);
            var bottomLeft = new VertexPositionColor(new Vector3(-x, -y, 0), Color.Magenta);
            var left = new VertexPositionColor(new Vector3(-size, 0, 0), Color.Red);


            return new[] {
                center,
                topLeft,
                topRight,
                right,
                bottomRight,
                bottomLeft,
                left
            };
        }

        public Hexagon(GraphicsDevice gd, float size=1.0f) {
            TriangleVertices = MakeHexagonVertices(Vector3.Zero, size);
            VertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColor), TriangleVertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData(TriangleVertices);
            IndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, HexagonIndices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(HexagonIndices);
        }
    }
}