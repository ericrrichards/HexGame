namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization.Formatters.Binary;

    using JetBrains.Annotations;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    using Newtonsoft.Json;

    public enum MeshType {
        Smooth,
        Flat
    }

    public class HexMap {
        public int Width { get; }
        public int Height { get; }



        public const float HexSize = 0.5f;
        public List<Hexagon> Hexes { get; }


        private const int PatchSize = 10;
        private List<HexMapMesh> Meshes { get; set; }
        private HashSet<int> DirtyPatches { get; } = new HashSet<int>();


        public bool ShowCoords { get; set; }
        public bool ShowGrid { get; set; }
        public bool Wireframe { get; set; }
        public bool ShowHexHeights { get; set; }

        public MeshType MeshType { get; }

        private readonly SpriteFont _font;

        private const float HeightStep = 0.25f;
        public Texture2D Texture { get; }
        

        public HexMap(GraphicsDevice gd, int width, int height, Texture2D texture, SpriteFont font = null, MeshType meshType=MeshType.Flat) {
            _font = font;
            Width = width;
            Height = height;
            Hexes = new List<Hexagon>();
            MeshType = meshType;

            Texture = texture;
            
            var hexHeight = HexMetrics.Height(HexSize);
            for (var x = 0; x < Width; x++) {
                for (var y = 0; y < Height; y++) {
                    var position = GetHexCenter(x, y, hexHeight);
                    var hexagon = new Hexagon(position, HexSize) {
                        MapPos = new Point(x, y)
                    };
                    Hexes.Add(hexagon);
                }
            }
            foreach (var hexagon in Hexes) {
                foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) {
                    hexagon.Neighbors[dir] = GetHex(HexMetrics.GetNeighborCoords(hexagon.MapPos, dir));
                }
                
            }

            Rebuild(gd, true);

        }

        private HexMap(GraphicsDevice gd, MapRecord record, ContentManager content, SpriteFont font=null) {
            _font = font;
            Width = record.Width;
            Height = record.Height;
            Hexes = new List<Hexagon>();
            MeshType = MeshType.Flat;
            Texture = content.Load<Texture2D>(record.BaseTexture);
            var hexHeight = HexMetrics.Height(HexSize);
            for (var x = 0; x < Width; x++) {
                for (var y = 0; y < Height; y++) {
                    var mapPos = new Point(x, y);
                    var position = GetHexCenter(x, y, hexHeight);
                    var hexRecord = record.Hexes.First(h => h.MapPos == mapPos);


                    var hexagon = new Hexagon(position, HexSize, hexRecord, HeightStep) {
                        MapPos = mapPos
                    };
                    Hexes.Add(hexagon);
                }
            }
            foreach (var hexagon in Hexes) {
                foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) {
                    hexagon.Neighbors[dir] = GetHex(HexMetrics.GetNeighborCoords(hexagon.MapPos, dir));
                }
                
            }

            Rebuild(gd, true);
        }

        private Vector3 GetHexCenter(int x, int y, float hexHeight) {
            var position = Vector3.Zero;
            position.X += 1.5f * HexSize * x;
            position.Z += hexHeight * y;
            if (x % 2 == 1) {
                position.Z += hexHeight / 2;
            }
            return position;
        }

        public void Rebuild(GraphicsDevice gd, bool force=false) {
            switch (MeshType) {
                case MeshType.Smooth:
                    Meshes = new List<HexMapMesh>{new HexMapMeshSmooth(gd, Hexes, Texture)};
                    break;
                case MeshType.Flat:
                    if (force) {
                        Meshes = new List<HexMapMesh>();
                        for (var xp = 0; xp < Width; xp += PatchSize) {
                            for (var yp = 0; yp < Height; yp += PatchSize) {
                                var patchHexes = new List<Hexagon>();
                                for (var x = xp; x < xp + PatchSize; x++) {
                                    for (var y = yp; y < yp + PatchSize; y++) {
                                        var hex = GetHex(x, y);
                                        if (hex != null) {
                                            patchHexes.Add(hex);
                                        }
                                    }
                                }
                                Meshes.Add(new HexMapMeshFlat(gd, patchHexes, Texture));


                            }
                        }
                    } else {
                        foreach (var dirtyPatch in DirtyPatches) {
                            var mesh = Meshes.FirstOrDefault(m => m.PatchID == dirtyPatch);
                            if (mesh == null) {
                                continue;
                            }
                            Meshes.Remove(mesh);
                            Meshes.Add(new HexMapMeshFlat(gd, mesh.Hexes, Texture));
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            DirtyPatches.Clear();
        }
        [CanBeNull]
        public Hexagon GetHex(Point p) {
            return GetHex(p.X, p.Y);
        }
        [CanBeNull]
        public Hexagon GetHex(int x, int y) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) {
                return null;
            }
            return Hexes[y + x * Width];
        }

        public void Draw(GraphicsDevice gd, BasicEffect effect, SpriteBatch spriteBatch, Camera camera) {
            foreach (var mesh in Meshes) {
                if (camera.Frustum.Intersects(mesh.BoundingBox)) {
                    mesh.DrawHexes(gd, effect, Wireframe);
                }
            }
            

            if (ShowGrid) {
                foreach (var mesh in Meshes) {
                    mesh.DrawGrid(gd, effect);
                }
                
            }
            if (ShowCoords) {
                DrawHexLabels(spriteBatch, camera, hex => $"{hex.MapPos.X}, {hex.MapPos.Y}");
            } else if (ShowHexHeights) {
                DrawHexLabels(spriteBatch, camera, hex => $"{hex.Position.Y/HeightStep}");
            }
        }

        private void DrawHexLabels(SpriteBatch spriteBatch, Camera camera, Func< Hexagon, string> displayFunc) {
            if (_font == null) {
                return;
            }
            spriteBatch.Begin();
            foreach (var hex in Hexes) {
                if (camera.Frustum.Contains(hex.Position) != ContainmentType.Contains) {
                    continue;
                }
                var projected = spriteBatch.GraphicsDevice.Viewport.Project(hex.Position, camera.ProjectionMatrix, camera.ViewMatrix, camera.WorldMatrix);
                var screen = new Vector2(projected.X, projected.Y);
                var pos = displayFunc(hex);
                var m = _font.MeasureString(pos);
                spriteBatch.DrawString(_font, pos, screen - m / 2, Color.White);
            }
            

            spriteBatch.End();
        }

        

        public Hexagon PickHex(Ray ray) {
            if (Meshes.All(m => ray.Intersects(m.BoundingBox) == null)) {
                return null;
            }
            var d = float.MaxValue;
            Hexagon ret = null;
            foreach (var hex in Hexes) {
                var td = hex.IntersectedBy(ray);
                if (td == null || !(td < d)) {
                    continue;
                }
                d = td.Value;
                ret = hex;

            }
            
            return ret;
        }

        public Vector3? PickVertex(Ray ray) {
            Vector3? vertex = null;
            var d = float.MaxValue;
            foreach (var mesh in Meshes) {
                if (ray.Intersects(mesh.BoundingBox) == null) {
                    continue;
                }
                var v = mesh.PickVertex(ray, out var td);
                if (v != null && td != null && td < d) {
                    d = td.Value;
                    vertex = v;
                }
            }
            return vertex;
        }
        
        private void RaiseVertex(Vector3 vertex, float dy) {
            var comparer = new Vector3Comparer();
            var affectedHexes = Hexes.Where(h => h.Points.Values.Any(v => comparer.Equals(v, vertex)))
                                     .Select(h => new {
                                         hex = h, 
                                         point = h.Points.First(p=>comparer.Equals(p.Value, vertex)).Key
                                      }).ToList();
            if (!affectedHexes.All(h => h.hex.CanRaisePoint(h.point, dy))) {
                return;
            }
            foreach (var affectedHex in affectedHexes) {
                affectedHex.hex.Raise(dy, affectedHex.point);
            }
            affectedHexes.Select(h => h.hex.PatchID).Distinct().ToList().ForEach(i => DirtyPatches.Add(i));
        }

        public void RaiseVertex(Vector3 vertex) {
            RaiseVertex(vertex, HeightStep);
        }

        public void LowerVertex(Vector3 vertex) {
            RaiseVertex(vertex, -HeightStep);
        }

        public void SaveToFile(string filename) {
            var record = new MapRecord {
                Name = Path.GetFileNameWithoutExtension(filename),
                Width = Width,
                Height = Height,
                BaseTexture = Texture.Name,
                Hexes = Hexes.Select(h => new HexRecord(h, HeightStep)).ToArray()
            };

            File.WriteAllText(filename, JsonConvert.SerializeObject(record));
        }

        public void SaveToFileBinary(string filename) {
            var record = new MapRecord {
                Name = Path.GetFileNameWithoutExtension(filename),
                Width = Width,
                Height = Height,
                BaseTexture = Texture.Name,
                Hexes = Hexes.Select(h => new HexRecord(h, HeightStep)).ToArray()
            };
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(filename)) {
                formatter.Serialize(stream, record);
            }
        }

        public static HexMap LoadFromFile(string filename, GraphicsDevice gd, ContentManager content, SpriteFont font=null) {
            var data = File.ReadAllText(filename);
            var record = JsonConvert.DeserializeObject<MapRecord>(data);
            return new HexMap(gd, record, content, font);
        }
        public static HexMap LoadFromFileBinary(string filename, GraphicsDevice gd, ContentManager content, SpriteFont font=null) {
            using (var stream = File.OpenRead(filename)) {
                var formatter = new BinaryFormatter();
                var record = (MapRecord)formatter.Deserialize(stream);
                return new HexMap(gd, record, content, font);
            }
            
        }

    }

    
}