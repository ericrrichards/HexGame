namespace HexGame {
    using System.Linq;

    using Microsoft.Xna.Framework;

    public class MapRecord {
        public string Name { get; set; }
        public Point Size { get; set; }
        public string BaseTexture {get; set; }
        public HexRecord[] Hexes { get; set; }
    }
    public struct HexRecord {
        //TODO could use some tricks to smoosh the size of this down some...
        public Point MapPos { get; set; }
        public int[] Heights { get; set; }

        public HexRecord(Hexagon hex, float heightStep = 0.25f) {
            MapPos = hex.MapPos;
            Heights = hex.Points.OrderBy(kv => kv.Key).Select(kv => (int)(kv.Value.Y / heightStep)).ToArray();
        }
    }
}