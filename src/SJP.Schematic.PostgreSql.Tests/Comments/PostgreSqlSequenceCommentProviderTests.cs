using System;
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

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlSequenceCommentProvider(null, identifierDefaults));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlSequenceCommentProvider(connection, null));
        }

        [Test]
        public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new PostgreSqlSequenceCommentProvider(connection, identifierDefaults);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetSequenceComments(null));
        }
    }
}
