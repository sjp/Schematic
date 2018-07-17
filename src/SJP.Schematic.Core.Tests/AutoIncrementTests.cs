using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class AutoIncrementTests
    {
        [Test]
        public static void InitialValue_PropertyGet_EqualsCtorArgument()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var autoIncrement = new AutoIncrement(initialValue, increment);

            Assert.AreEqual(initialValue, autoIncrement.InitialValue);
        }

        [Test]
        public static void Increment_PropertyGet_EqualsCtorArgument()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var autoIncrement = new AutoIncrement(initialValue, increment);

            Assert.AreEqual(increment, autoIncrement.Increment);
        }
    }
}
