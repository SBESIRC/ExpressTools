using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameIEnumerableExtensionTest
    {

        [Test]
        public void TestSequences()
        {
            var list = new List<int> { 1, 2, 3, 6, 7, 8, 9 };
            var results = list.ConsecutiveSequences();
            Assert.AreEqual(results.First(), new List<int> { 1, 2, 3 });
            Assert.AreEqual(results.Last(), new List<int> { 6, 7, 8, 9 });
        }

        [Test]
        public void TestOddSequences()
        {
            var list = new List<int> { 1, 3, 5, 6, 8, 9 };
            var results = list.OddSequences();
            Assert.AreEqual(results.First(), new List<int> { 1, 3, 5 });
        }
    }
}
