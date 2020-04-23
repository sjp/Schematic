using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments
{
    [TestFixture]
    internal static class SqlServerDatabaseCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new SqlServerDatabaseCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();

            Assert.That(() => new SqlServerDatabaseCommentProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetTableComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetViewComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetSequenceComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonymComments_GivenNullSynonymName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetSynonymComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutineComments_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnectionFactory>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new SqlServerDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetRoutineComments(null), Throws.ArgumentNullException);
        }
    }
}
