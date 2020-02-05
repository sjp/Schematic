using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDbTypeProviderTests
    {
        // TODO
        [Test]
        public static void Ctor_GivenNoComparers_CreatesWithoutError()
        {
            Assert.That(() => new SqliteDbTypeProvider(), Throws.Nothing);
        }
    }
}
