using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.SqlServer.Comments;
using System.Text;

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments
{
    internal sealed class SqlServerSynonymCommentProviderTests : SqlServerTest
    {
        private IDatabaseSynonymCommentProvider SynonymCommentProvider => new SqlServerSynonymCommentProvider(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view synonym_comment_view_1 as select 1 as test_column_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create synonym synonym_comment_synonym_1 for synonym_comment_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create synonym synonym_comment_synonym_2 for synonym_comment_view_1").ConfigureAwait(false);

            await AddCommentForSynonym("This is a test synonym comment.", "dbo", "synonym_comment_synonym_2").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop synonym synonym_comment_synonym_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop synonym synonym_comment_synonym_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view synonym_comment_view_1").ConfigureAwait(false);
        }

        private Task AddCommentForSynonym(string comment, string schemaName, string synonymName)
        {
            const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'SYNONYM',
  @level1name = @SynonymName";
            return Connection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, SynonymName = synonymName });
        }

        private Task<IDatabaseSynonymComments> GetSynonymCommentsAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            lock (_lock)
            {
                if (!_commentsCache.TryGetValue(synonymName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseSynonymComments>(() => SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync());
                    _commentsCache[synonymName] = lazyComment;
                }

                return lazyComment.Task;
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseSynonymComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseSynonymComments>>();

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresent_ReturnsSynonymComment()
        {
            var synonymIsSome = await SynonymCommentProvider.GetSynonymComments("synonym_comment_synonym_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(synonymIsSome);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            const string synonymName = "synonym_comment_synonym_1";
            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(synonymName, synonymComments.SynonymName.LocalName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonymComments.SynonymName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonymComments.SynonymName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonymComments.SynonymName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(synonymName, synonymComments.SynonymName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonymComments.SynonymName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonymComments.SynonymName);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymMissing_ReturnsNone()
        {
            var synonymIsNone = await SynonymCommentProvider.GetSynonymComments("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(synonymIsNone);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("SYNONYM_COMMENT_synonym_1");
            var synonymComments = await SynonymCommentProvider.GetSynonymComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonymComments.SynonymName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "SYNONYM_COMMENT_synonym_1");
            var synonymComments = await SynonymCommentProvider.GetSynonymComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonymComments.SynonymName.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonymComments.SynonymName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetAllSynonymComments_WhenEnumerated_ContainsSynonymComments()
        {
            var hasSynonymComments = await SynonymCommentProvider.GetAllSynonymComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsTrue(hasSynonymComments);
        }

        [Test]
        public async Task GetAllSynonymComments_WhenEnumerated_ContainsTestSynonymComment()
        {
            var containsTestSynonym = await SynonymCommentProvider.GetAllSynonymComments()
                .AnyAsync(t => t.SynonymName.LocalName == "synonym_comment_synonym_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestSynonym);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymMissingComment_ReturnsNone()
        {
            var comments = await GetSynonymCommentsAsync("synonym_comment_synonym_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test synonym comment.";
            var comments = await GetSynonymCommentsAsync("synonym_comment_synonym_2").ConfigureAwait(false);

            var synonymComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, synonymComment);
        }
    }
}
