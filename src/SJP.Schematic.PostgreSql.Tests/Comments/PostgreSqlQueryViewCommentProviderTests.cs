using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments
{
    [TestFixture]
    internal static class PostgreSqlQueryViewCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlQueryViewCommentProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            Assert.That(() => new PostgreSqlQueryViewCommentProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new PostgreSqlQueryViewCommentProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            var commentProvider = new PostgreSqlQueryViewCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.That(() => commentProvider.GetViewComments(null), Throws.ArgumentNullException);
        }
    }
}
