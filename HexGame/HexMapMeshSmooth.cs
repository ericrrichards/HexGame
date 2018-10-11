namespace HexGame {
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class HexMapMeshSmooth : HexMapMesh {
        public HexMapMeshSmooth(GraphicsDevice gd, List<Hexagon> hexes, Texture2D texture) : base(gd, hexes, new SmoothGeometryBuilder(), texture) {
            
        }

        private class SmoothGeometryBuilder : IGeometryBuilder {
            public void BuildGeometry(List<Hexagon> hexes, List<VertexPositionNormalTexture> vertices, List<uint> indices) {
                uint i = 0;
                var vertToIndex = new Dictionary<Vector3, uint>(new Vector3Comparer());
                foreach (var hexagon in hexes) {
                    foreach (var tri in hexagon.Geometry.Triangles) {
                        if (!vertToIndex.ContainsKey(tri.P0)) {
                            vertToIndex[tri.P0] = i++;
                            vertices.Add(new VertexPositionNormalTexture(tri.P0, Vector3.Up, tri.UV0));
                        }
                        if (!vertToIndex.ContainsKey(tri.P1)) {
                            vertToIndex[tri.P1] = i++;
                            vertices.Add(new VertexPositionNormalTexture(tri.P1, Vector3.Up, tri.UV1));
                        }
                        if (!vertToIndex.ContainsKey(tri.P2)) {
                            vertToIndex[tri.P2] = i++;
                            vertices.Add(new VertexPositionNormalTexture(tri.P2, Vector3.Up, tri.UV2));
                        }
                    }
                    foreach (var c in HexMetrics.IndexOrder) {
                        var p = hexagon.Geometry.Points[c];
                        indices.Add(vertToIndex[p]);
                    }
                }
            }

            public void GenerateNormals(VertexPositionNormalTexture[] vertices, List<uint> indices) {
                for (var i = 0; i < vertices.Length; i++) {
                    vertices[i].Normal = Vector3.UnitY;
                }
                for (var i = 0; i < indices.Count; i+=3) {
                    var v1 = vertices[indices[i]].Position - vertices[indices[i + 1]].Position;
                    var v2 = vertices[indices[i + 2]].Position - vertices[indices[i +1]].Position;
                    var normal = Vector3.Cross(v1, v2);
                    normal.Normalize();
                    vertices[indices[i ]].Normal += normal;
                    vertices[indices[i + 1]].Normal += normal;
                    vertices[indices[i + 2]].Normal += normal;
                }
                for (var i = 0; i < vertices.Length; i++) {
                    vertices[i].Normal.Normalize();
                }
            }
        }

        
        

    }
}