namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public abstract class HexMapMesh {
        public interface IGeometryBuilder {
            void BuildGeometry(List<Hexagon> hexes, List<Vector3> vertices, List<uint> indices);
            void GenerateNormals(VertexPositionColorNormal[] vertices, List<uint> indices);
        }

        protected List<Vector3> Vertices { get; }
        protected List<uint> Indices { get; }

        protected VertexBuffer VertexBuffer { get; }
        protected IndexBuffer IndexBuffer { get; }

        public int TriangleCount { get; }
        protected float HexSize { get; }

        public BoundingBox BoundingBox;

        // TODO this is temporary
        protected float MaxHeight { get; } = 5;

        protected HexMapMesh(GraphicsDevice gd, List<Hexagon> hexes, IGeometryBuilder builder) {
            Vertices = new List<Vector3>();
            Indices = new List<uint>();
            TriangleCount = hexes.Count * 6;
            HexSize = hexes[0].HexWidth;

            builder.BuildGeometry(hexes, Vertices, Indices);

            BoundingBox = BoundingBox.CreateFromPoints(Vertices);

            VertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColorNormal), Vertices.Count, BufferUsage.WriteOnly);
            var vpcs = Vertices.Select(v => new VertexPositionColorNormal(v, GetColor(v), Vector3.Up)).ToArray();
            builder.GenerateNormals(vpcs, Indices);
            VertexBuffer.SetData(vpcs);
            IndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());
        }
        private Color GetColor(Vector3 v) {
            const float floor = 0.25f;
            const float ceiling = 1.0f;
            const float median = (floor + ceiling) / 2;
            var colorStep = ceiling - floor / (MaxHeight * 2);
            var value = MathHelper.Clamp(median + v.Y / MaxHeight * colorStep, floor, ceiling);


            return new Color(0, value, 0);
        }
        public void DrawHexes(GraphicsDevice gd, BasicEffect effect, bool wireframe=false) {
            gd.SetVertexBuffer(VertexBuffer);
            gd.Indices = IndexBuffer;
            var rs = gd.RasterizerState;
            if (wireframe) {
                gd.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame };
            }


            effect.LightingEnabled = true;
            
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
        public Vector3? PickVertex(Ray ray) {
            if (ray.Intersects(BoundingBox) == null) {
                return null;
            }
            var d = float.MaxValue;
            Vector3? ret = null;
            var sphere = new BoundingSphere(Vector3.Zero, HexSize/3);
            foreach (var vertex in Vertices) {
                var td = ray.Intersects(sphere.Transform(Matrix.CreateTranslation(vertex)));
                if (td == null || !(td < d)) {
                    continue;
                }
                d = td.Value;
                ret = vertex;
            }

            return ret;
        }

    }
}