using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeAutotestsSuite
{
    [TestClass]
    public sealed class FakeSuite
    {
        private const string QcId = nameof(QcId);

        [TestMethod]
        public void FakeTestOne()
        {

        }

        [TestMethod]
        public void FakeTestTwo()
        {

        }

        [TestMethod]
        [TestProperty(QcId, "1643013")]
        public void UngroupTest1()
        {

        }

        [TestMethod]
        [TestProperty(QcId, "1643014")]
        public void UngroupTest2()
        {

        }

        [TestMethod]
        [TestProperty(QcId, "1643014")]
        public void UngroupTest3()
        {

        }

        [TestMethod]
        [TestProperty(QcId, "1643015")]
        public void UngroupTest4()
        {

        }
    } 
}