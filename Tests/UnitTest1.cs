using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zadachka;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test2x2()
        {
            var parser = new MathParser("2*2");
            Assert.AreEqual(parser.calculate(), 4);
        }

        [TestMethod]
        public void TestSimplify()
        {
            var parser = new MathParser("2*x+3*x");
            parser.makeSimple();
            var simple = parser.assembly();
            Assert.AreEqual(simple, "5*x");
        }
    }
}
