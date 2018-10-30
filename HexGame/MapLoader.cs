namespace HexGame {
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    using Newtonsoft.Json;

    using ProtoBuf;

    public class MapLoader {
        public void SaveToFile(string filename, HexMap map) {
            var record = new MapRecord {
                Name = Path.GetFileNameWithoutExtension(filename),
                Width = map.Width,
                Height = map.Height,
                BaseTexture = map.Texture.Name,
                Hexes = map.Hexes.Select(h => new HexRecord(h)).ToArray()
            };

            File.WriteAllText(filename, JsonConvert.SerializeObject(record));
        }
        public void SaveToFileBinary(string filename, HexMap map) {
            var record = new MapRecord {
                Name = Path.GetFileNameWithoutExtension(filename),
                Width = map.Width,
                Height = map.Height,
                BaseTexture = map.Texture.Name,
                Hexes = map.Hexes.Select(h => new HexRecord(h)).ToArray()
            };
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(filename)) {
                formatter.Serialize(stream, record);
            }
        }

        public void SaveToFileProto(string filename, HexMap map) {
            var record = new MapRecord {
                Name = Path.GetFileNameWithoutExtension(filename),
                Width = map.Width,
                Height = map.Height,
                BaseTexture = map.Texture.Name,
                Hexes = map.Hexes.Select(h => new HexRecord(h)).ToArray()
            };
            using (var stream = File.OpenWrite(filename)) {
                Serializer.Serialize(stream, record);
            }
        }
        public HexMap LoadFromFile(string filename, GraphicsDevice gd, ContentManager content, SpriteFont font=null) {
            var data = File.ReadAllText(filename);
            var record = JsonConvert.DeserializeObject<MapRecord>(data);
            return new HexMap(gd, record, content, font);
        }
        public HexMap LoadFromFileBinary(string filename, GraphicsDevice gd, ContentManager content, SpriteFont font=null) {
            using (var stream = File.OpenRead(filename)) {
                var formatter = new BinaryFormatter();
                var record = (MapRecord)formatter.Deserialize(stream);
                return new HexMap(gd, record, content, font);
            }
            
        }

        public HexMap LoadFromFileProto(string filename, GraphicsDevice gd, ContentManager content, SpriteFont font = null) {
            using (var stream = File.OpenRead(filename)) {
                var record = Serializer.Deserialize<MapRecord>(stream);
                return new HexMap(gd, record, content, font);
            }
        }
    }
}