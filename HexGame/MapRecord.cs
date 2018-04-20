namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Microsoft.Xna.Framework;

    using ProtoBuf;

    [Serializable]
    [ProtoContract]
    public class MapRecord {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public int Width { get; set; }
        [ProtoMember(3)]
        public int Height { get;set; }
        [ProtoMember(4)]
        public string BaseTexture {get; set; }
        [ProtoMember(5)]
        public HexRecord[] Hexes { get; set; }
    }
    [Serializable]
    [DataContract]
    [ProtoContract]
    public struct HexRecord {
        public Point MapPos {
            get => ToPoint();
            set => Pos = ToShort(value);
        }

        public Point ToPoint() {
            return new Point(Pos & 0xFF, Pos >> 8);
        }

        [DataMember]
        [ProtoMember(1)]
        public ushort Pos { get;set; }
        [DataMember]
        [ProtoMember(2)]
        public ulong H { get; set; }

        [DataMember]
        [ProtoMember(3)]
        public byte Mod { get; set; }
        public bool Forested => Mod == 1;

        public int[] Heights {
            get => ToIntArray(H);
            set => H = ToLong(value);
        }

        public HexRecord(Hexagon hex, float heightStep = 0.25f) {
            Pos = ToShort(hex.MapPos);
            H = ToLong(hex.Points.OrderBy(kv => kv.Key).Select(kv => (int)(kv.Value.Y / heightStep)).ToArray());
            Mod = (byte)(hex.IsForest ? 1 : 0);
        }

        public static ushort ToShort(Point value) {
            return (ushort)(value.X | value.Y << 8);
        }

        public static ulong ToLong(int[] values) {
            ulong ret = 0;
            for (var i = 0; i < values.Length && i < 8; i++) {
                ret |= (ulong)(values[i] + 127) << (8 * i);
            }
            return ret;
        }

        public static int[] ToIntArray(ulong value) {
            var ret = new List<int>();

            for (var i = 0; i < (int)(HexagonPoint.Left+1); i++) {
                ret.Add((int)(value >> i*8 & 0xff) - 127);
            }

            return ret.ToArray();
        }
    }
}