namespace HexGame {
    using NUnit.Framework;

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