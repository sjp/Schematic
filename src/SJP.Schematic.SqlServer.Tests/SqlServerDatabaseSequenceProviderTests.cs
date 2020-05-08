using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseSequenceProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerDatabaseSequenceProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();

            Assert.That(() => new SqlServerDatabaseSequenceProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var sequenceProvider = new SqlServerDatabaseSequenceProvider(connection, identifierDefaults);

            Assert.That(() => sequenceProvider.GetSequence(null), Throws.ArgumentNullException);
        }
    }
}
