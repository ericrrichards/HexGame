namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class HexMapMesh {
        private List<Vector3> Vertices { get; }
        private List<uint> Indices { get; }

        private VertexBuffer VertexBuffer { get; }
        private IndexBuffer IndexBuffer { get; }

        public int TriangleCount { get; }
        private float HexSize { get; }

        public BoundingBox BoundingBox;

        // TODO this is temporary
        private float MaxHeight { get; } = 5;

        public HexMapMesh(GraphicsDevice gd, List<Hexagon> hexes) {
            Vertices = new List<Vector3>();
            Indices = new List<uint>();
            TriangleCount = hexes.Count * 6;
            HexSize = hexes[0].HexWidth;
            uint i = 0;
            var vertToIndex = new Dictionary<Vector3, uint>(new Vector3Comparer());

            
            foreach (var hexagon in hexes) {
                foreach (var tri in hexagon.Triangles) {
                    if (!vertToIndex.ContainsKey(tri.P0)) {
                        vertToIndex[tri.P0] = i++;
                        Vertices.Add(tri.P0);
                    }
                    if (!vertToIndex.ContainsKey(tri.P1)) {
                        vertToIndex[tri.P1] = i++;
                        Vertices.Add(tri.P1);
                    }
                    if (!vertToIndex.ContainsKey(tri.P2)) {
                        vertToIndex[tri.P2] = i++;
                        Vertices.Add(tri.P2);
                    }
                }
                foreach (var c in HexMetrics.IndexOrder) {
                    var p = hexagon.Points[c];
                    Indices.Add(vertToIndex[p]);
                }
            }
            
            BoundingBox = BoundingBox.CreateFromPoints(Vertices);
            

            VertexBuffer = new VertexBuffer(gd, typeof(VertexPositionColorNormal), Vertices.Count, BufferUsage.WriteOnly);
            var vpcs = Vertices.Select(v => new VertexPositionColorNormal(v, GetColor(v), Vector3.Up)).ToArray();
            GenerateNormals(vpcs, Indices);
            VertexBuffer.SetData(vpcs);
            IndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, Indices.Count, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices.ToArray());
        }
        private void GenerateNormals(VertexPositionColorNormal[] vertices, List<uint> indices) {
            for (var i = 0; i < vertices.Length; i++) {
                vertices[i].Normal = Vector3.UnitY;
            }
            for (var i = 0; i < TriangleCount; i++) {
                var v1 = vertices[indices[i * 3]].Position - vertices[indices[i * 3 + 1]].Position;
                var v2 = vertices[indices[i * 3 + 2]].Position - vertices[indices[i * 3+1]].Position;
                var normal = Vector3.Cross(v1, v2);
                normal.Normalize();
                vertices[indices[i * 3]].Normal += normal;
                vertices[indices[i * 3 + 1]].Normal += normal;
                vertices[indices[i * 3 + 2]].Normal += normal;
            }
            for (var i = 0; i < vertices.Length; i++) {
                vertices[i].Normal.Normalize();
            }
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
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1, -1, 0));
            effect.DirectionalLight0.DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f);

            effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);

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