using NUnit.Framework;
using Zenject;

namespace Project.Scripts.UnitTests.Editor.Tests
{
    [TestFixture]
    public class DummyManagerTest: ZenjectUnitTestFixture
    {
        [Test]
        public void TestDummy()
        {
            Assert.IsTrue(true);
        }
    }
}
