using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;
using SJP.Schematic.MySql.Comments;

namespace SJP.Schematic.MySql.Tests.Comments
{
    [TestFixture]
    internal static class MySqlDatabaseCommentProviderTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            Assert.That(() => new MySqlDatabaseCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => new MySqlDatabaseCommentProvider(connection, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new MySqlDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetTableComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new MySqlDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetViewComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new MySqlDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetSequenceComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetSynonymComments_GivenNullSynonymName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new MySqlDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetSynonymComments(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetRoutineComments_GivenNullRoutineName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            var commentProvider = new MySqlDatabaseCommentProvider(connection, identifierDefaults);

            Assert.That(() => commentProvider.GetRoutineComments(null), Throws.ArgumentNullException);
        }
    }
}
