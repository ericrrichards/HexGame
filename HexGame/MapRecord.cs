namespace HexGame {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Microsoft.Xna.Framework;

    using NUnit.Framework;

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
        //TODO could use some tricks to smoosh the size of this down some...
        public Point MapPos {
            get => new Point(Pos & 0xFF, Pos >> 8);
            set => Pos = ToShort(value);
        }

        
        
        [DataMember]
        [ProtoMember(1)]
        public ushort Pos { get;set; }
        [DataMember]
        [ProtoMember(2)]
        public ulong H { get; set; }

        public int[] Heights {
            get => ToIntArray(H);
            set => H = ToLong(value);
        }

        public HexRecord(Hexagon hex, float heightStep = 0.25f) {
            Pos = ToShort(hex.MapPos);
            H = ToLong(hex.Points.OrderBy(kv => kv.Key).Select(kv => (int)(kv.Value.Y / heightStep)).ToArray());
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

    [TestFixture]
    public class HexRecordTests {
        [TestCase(new[]{0,0,0,0,0,0,0}, 35887507618889599U)]
        [TestCase(new[]{-1,0,0,0,0,0,0}, 35887507618889598U)]
        [TestCase(new[]{1,0,0,0,0,0,0}, 35887507618889600U)]
        public void ToLong(int[] input, ulong expected) {
            Assert.AreEqual(expected, HexRecord.ToLong(input));
        }
        [TestCase(35887507618889599U, new[]{0,0,0,0,0,0,0})]
        [TestCase(35887507618889600U, new[]{1,0,0,0,0,0,0})]
        [TestCase(35887507618889598U, new[]{-1,0,0,0,0,0,0})]
        public void ToIntArray(ulong input, int[] expected) {
            Assert.AreEqual(expected, HexRecord.ToIntArray(input));
        }
    }
}