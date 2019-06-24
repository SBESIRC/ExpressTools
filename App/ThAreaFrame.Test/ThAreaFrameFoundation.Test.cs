using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameFoundationTest
    {
        [Test]
        public void CreateObject()
        {
            ThAreaFrameFoundation foundation = ThAreaFrameFoundation.Foundation(@"单体基底_1_1号单体_混合_4_2_2.8%-2^-1'2!4%1!4.5%3'4!3.6_0_0.45_1.2_是__V2.2");
            Assert.AreEqual(foundation.numberID, @"1");
            Assert.AreEqual(foundation.name, @"1号单体");
            Assert.AreEqual(foundation.category, @"混合");
        }

        [Test]
        public void StoreyHeight()
        {
            ThAreaFrameFoundation foundation = ThAreaFrameFoundation.Foundation(@"单体基底_1_1号单体_混合_4_2_2.8%-2^-1'2!4%1!4.5%3'4!3.6_0_0.45_1.2_是__V2.2");
            Assert.AreEqual(foundation.StoreyHeight(-2), 4.0);
            Assert.AreEqual(foundation.StoreyHeight(-1), 4.0);
            Assert.AreEqual(foundation.StoreyHeight(1), 4.5);
            Assert.AreEqual(foundation.StoreyHeight(2), 4.0);
            Assert.AreEqual(foundation.StoreyHeight(3), 3.6);
            Assert.AreEqual(foundation.StoreyHeight(4), 3.6);
            Assert.AreEqual(foundation.StoreyHeight(5), 2.8);
        }
    }
}