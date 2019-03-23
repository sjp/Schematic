using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments
{
    [TestFixture]
    internal static class PostgreSqlTableCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlTableCommentProvider(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlTableCommentProvider(connection, null));
        }

        [Test]
        public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new PostgreSqlTableCommentProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetTableComments(null));
        }
    }
}
