namespace HexGame {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class HexGrid {
        private Color Color { get; }
        private VertexBuffer GridVertexBuffer { get; }
        private IndexBuffer GridIndexBuffer { get; }
        private int GridIndexCount { get; }


        public HexGrid(GraphicsDevice gd, IEnumerable<Hexagon> hexes, Color color) {
            Color = color;
            var verts = new List<VertexPositionColor>();
            var indices = new List<uint>();
            uint i = 0;
            foreach (var hex in hexes) {
                var borderVerts = hex.Geometry.Border;
                verts.AddRange(borderVerts.Select(v => new VertexPositionColor(v + new Vector3(0, .01f, 0), Color)));
                indices.Add(i);
                indices.Add(i + 1);

                indices.Add(i + 1);
                indices.Add(i + 2);

                indices.Add(i + 2);
                indices.Add(i + 3);

                indices.Add(i + 3);
                indices.Add(i + 4);

                indices.Add(i + 4);
                indices.Add(i + 5);

                indices.Add(i + 5);
                indices.Add(i);

                i += 6;
            }
            
            GridIndexCount = indices.Count;
            GridIndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            GridIndexBuffer.SetData(indices.ToArray());
            GridVertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);
            GridVertexBuffer.SetData(verts.ToArray());
        }

        public void DrawGrid(GraphicsDevice gd, BasicEffect effect) {
            gd.SetVertexBuffer(GridVertexBuffer);
            gd.Indices = GridIndexBuffer;
            

            var oldLighting = effect.LightingEnabled;
            effect.LightingEnabled = false;
            var oldTexturing = effect.TextureEnabled;
            effect.TextureEnabled = false;
            var oldVertColoring = effect.VertexColorEnabled;
            effect.VertexColorEnabled = true;
            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                gd.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, GridIndexCount / 2);
            }
            effect.LightingEnabled = oldLighting;
            effect.VertexColorEnabled = oldVertColoring;
            effect.TextureEnabled = oldTexturing;
        }
    }
}