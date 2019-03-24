using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.Oracle.Comments;

namespace SJP.Schematic.Oracle.Tests.Comments
{
    [TestFixture]
    internal static class OracleMaterializedViewCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleMaterializedViewCommentProvider(null, identifierDefaults, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleMaterializedViewCommentProvider(connection, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new OracleMaterializedViewCommentProvider(connection, identifierDefaults, null));
        }

        [Test]
        public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var commentProvider = new OracleMaterializedViewCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetViewComments(null));
        }
    }
}
