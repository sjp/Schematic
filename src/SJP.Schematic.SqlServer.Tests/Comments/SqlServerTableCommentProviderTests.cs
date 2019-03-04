using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments
{
    [TestFixture]
    internal static class SqlServerTableCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerTableCommentProvider(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerTableCommentProvider(connection, null));
        }

        [Test]
        public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerTableCommentProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetTableComments(null));
        }
    }
}
