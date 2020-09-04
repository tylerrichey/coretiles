using CoreTiles.Tiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class TilesInterfaceTests
    {
        [TestMethod]
        public void TestLimitedList()
        {
            var list = new LimitedList<string>(10);
            foreach (var i in Enumerable.Range(0, 10))
            {
                Assert.IsTrue(list.TryAdd(i.ToString()));
            }
            Assert.AreEqual(10, list.Count);
            list.TryAdd("11");
            Assert.AreEqual(10, list.Count);
            Assert.AreEqual("1", list.First());
            Assert.AreEqual("11", list.Last());
        }
    }
}
