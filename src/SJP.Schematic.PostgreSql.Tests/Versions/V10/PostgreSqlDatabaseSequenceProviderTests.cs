using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SequenceProvider = SJP.Schematic.PostgreSql.Versions.V10.PostgreSqlDatabaseSequenceProvider;

namespace SJP.Schematic.PostgreSql.Tests.Versions.V10
{
    [TestFixture]
    internal static class PostgreSqlDatabaseSequenceProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new SequenceProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            Assert.That(() => new SequenceProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SequenceProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequence_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

            var sequenceProvider = new SequenceProvider(connection, identifierDefaults, identifierResolver);

            Assert.That(() => sequenceProvider.GetSequence(null), Throws.ArgumentNullException);
        }
    }
}
