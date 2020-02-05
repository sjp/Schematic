using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments
{
    [TestFixture]
    internal static class PostgreSqlSequenceCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlSequenceCommentProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlSequenceCommentProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new PostgreSqlSequenceCommentProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var commentProvider = new PostgreSqlSequenceCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.That(() => commentProvider.GetSequenceComments(null), Throws.ArgumentNullException);
        }
    }
}
