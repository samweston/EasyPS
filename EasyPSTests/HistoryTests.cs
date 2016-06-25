using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

namespace EasyPS.Tests
{
    [TestClass()]
    public class HistoryTests
    {
        string TempDirectory { get; set; }
        IHistory History { get; set; }
        public object Settings { get; private set; }

        [TestInitialize()]
        public void HistoryTestsInit()
        {
            string dir = Directory.GetCurrentDirectory();
            TempDirectory = dir + "\\temp";
            Directory.CreateDirectory(TempDirectory);

            //History = new HistoryXML();
            History = new HistorySetting();
            //History.SetSavePath(TempDirectory);
        }

        [TestCleanup()]
        public void HistoryTestsCleanup()
        {
            //Directory.Delete(TempDirectory, true);
        }

        [TestMethod()]
        public void TestNull()
        {
            var items = History.GetHistory("path\\to\\file1", "prop1");
            Assert.IsNull(items);
        }

        [TestMethod()]
        public void TestAddOne()
        {
            string path = "path\\to\\file2";
            string prop = "prop2";
            string val = "value2";
            History.AddHistory(path, prop, val);
            var items = History.GetHistory(path, prop);
            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(val, items.First());
        }

        [TestMethod()]
        public void TestAddTwo()
        {
            string path = "path\\to\\file3";
            string prop = "prop3";
            string val1 = "value3";
            string val2 = "value3_2";
            History.AddHistory(path, prop, val1);
            History.AddHistory(path, prop, val2);
            History.AddHistory(path, prop, val2); // shouldn't be added
            var items = History.GetHistory(path, prop);
            Assert.AreEqual(2, items.Count());
            Assert.AreEqual(val2, items.ToArray()[0]); // add inserts.
            Assert.AreEqual(val1, items.ToArray()[1]);
        }
    }
}