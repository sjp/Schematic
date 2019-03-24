using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.Oracle.Comments;

namespace SJP.Schematic.Oracle.Tests.Comments
{
    [TestFixture]
    internal static class OracleDatabaseCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseCommentProvider(null, identifierDefaults, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseCommentProvider(connection, null, identifierResolver));
        }

        [Test]
        public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseCommentProvider(connection, identifierDefaults, null));
        }

        [Test]
        public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var commentProvider = new OracleDatabaseCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetTableComments(null));
        }

        [Test]
        public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var commentProvider = new OracleDatabaseCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetViewComments(null));
        }

        [Test]
        public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var commentProvider = new OracleDatabaseCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetSequenceComments(null));
        }

        [Test]
        public static void GetSynonymComments_GivenNullSynonymName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var commentProvider = new OracleDatabaseCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetSynonymComments(null));
        }

        [Test]
        public static void GetRoutineComments_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var commentProvider = new OracleDatabaseCommentProvider(connection, identifierDefaults, identifierResolver);

            Assert.Throws<ArgumentNullException>(() => commentProvider.GetRoutineComments(null));
        }
    }
}
