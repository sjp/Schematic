using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class RelationalDatabaseTests
    {
        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
        {
            IIdentifierDefaults identifierDefaults = null;

            Assert.That(() => new FakeRelationalDatabase(identifierDefaults), Throws.ArgumentNullException);
        }

        private sealed class FakeRelationalDatabase : RelationalDatabase
        {
            public FakeRelationalDatabase(IIdentifierDefaults identifierDefaults)
                : base(identifierDefaults)
            {
            }
        }
    }
}
