namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public abstract class HexMapMesh {
        public interface IGeometryBuilder {
            void BuildGeometry(List<Hexagon> hexes, List<VertexPositionNormalTexture> vertices, List<uint> indices);
            void GenerateNormals(VertexPositionNormalTexture[] vertices, List<uint> indices);
        }

        private static int meshCounter;

        public int PatchID { get; }
        protected List<VertexPositionNormalTexture> Vertices { get; }
        protected List<uint> Indices { get; }

        protected VertexBuffer VertexBuffer { get; }
        protected IndexBuffer IndexBuffer { get; }

        public int TriangleCount { get; }
        protected float HexSize { get; }

        public BoundingBox BoundingBox;
        public List<Hexagon> Hexes { get; }

        
        private Texture2D Texture { get; }

        private HexGrid Grid { get; }

        protected HexMapMesh(GraphicsDevice gd, List<Hexagon> hexes, IGeometryBuilder builder, Texture2D texture) {
            Interlocked.Increment(ref meshCounter);
            Texture = texture;
            Hexes = hexes;
            PatchID = meshCounter;
            Vertices = new List<VertexPositionNormalTexture>();
            Indices = new List<uint>();
            TriangleCount = hexes.Count * 6;
            HexSize = hexes[0].HexWidth;
            foreach (var hexagon in hexes) {
                hexagon.PatchID = PatchID;
            }

            builder.BuildGeometry(hexes, Vertices, Indices);

            BoundingBox = hexes[0].BoundingBox;
            foreach (var hexagon in hexes.Skip(1)) {
                BoundingBox = BoundingBox.CreateMerged(BoundingBox, hexagon.BoundingBox);
            }

            VertexBuffer = new VertexBuffer(gd, typeof(VertexPositionNormalTexture), Vertices.Count, BufferUsage.WriteOnly);
            var vpcs = Vertices.ToArray();
            builder.GenerateNormals(vpcs, Indices);
            VertexBuffer.SetData(vpcs);
            IndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());
            Grid = new HexGrid(gd, Hexes, Color.Gray);
        }

        public void DrawHexes(GraphicsDevice gd, BasicEffect effect, bool wireframe=false) {
            gd.SetVertexBuffer(VertexBuffer);
            gd.Indices = IndexBuffer;
            var rs = gd.RasterizerState;
            if (wireframe) {
                gd.RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
            }


            effect.LightingEnabled = true;
            effect.TextureEnabled = true;
            effect.Texture = Texture;
            effect.Alpha = 1.0f;
            
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(0, -1, 1));
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.7f, 0.7f, 0.7f);

            effect.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);

            foreach (var pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TriangleCount);
            }
            gd.RasterizerState = rs;
        }
        public Vector3? PickVertex(Ray ray, out float? distance) {
            distance = null;
            if (ray.Intersects(BoundingBox) == null) {
                return null;
            }
            var d = float.MaxValue;
            Vector3? ret = null;
            var sphere = new BoundingSphere(Vector3.Zero, HexSize/3);
            foreach (var vertex in Vertices) {
                var td = ray.Intersects(sphere.Transform(Matrix.CreateTranslation(vertex.Position)));
                if (td == null || !(td < d)) {
                    continue;
                }
                d = td.Value;
                ret = vertex.Position;
            }
            distance = d;
            return ret;
        }

        public void DrawGrid(GraphicsDevice gd, BasicEffect effect) {
            Grid.DrawGrid(gd, effect);
        }
    }
}