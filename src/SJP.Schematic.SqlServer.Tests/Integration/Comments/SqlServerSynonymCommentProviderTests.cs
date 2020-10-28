using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments
{
    internal sealed class SqlServerSynonymCommentProviderTests : SqlServerTest
    {
        private IDatabaseSynonymCommentProvider SynonymCommentProvider => new SqlServerSynonymCommentProvider(DbConnection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync("create view synonym_comment_view_1 as select 1 as test_column_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create synonym synonym_comment_synonym_1 for synonym_comment_view_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create synonym synonym_comment_synonym_2 for synonym_comment_view_1", CancellationToken.None).ConfigureAwait(false);

            await AddCommentForSynonym("This is a test synonym comment.", "dbo", "synonym_comment_synonym_2").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop synonym synonym_comment_synonym_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop synonym synonym_comment_synonym_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop view synonym_comment_view_1", CancellationToken.None).ConfigureAwait(false);
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
            return DbConnection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, SynonymName = synonymName }, CancellationToken.None);
        }

        private Task<IDatabaseSynonymComments> GetSynonymCommentsAsync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return GetSynonymCommentsAsyncCore(synonymName);
        }

        private async Task<IDatabaseSynonymComments> GetSynonymCommentsAsyncCore(Identifier synonymName)
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
            {
                if (!_commentsCache.TryGetValue(synonymName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseSynonymComments>(() => SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync());
                    _commentsCache[synonymName] = lazyComment;
                }

                return await lazyComment.ConfigureAwait(false);
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseSynonymComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseSynonymComments>>();

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresent_ReturnsSynonymComment()
        {
            var synonymIsSome = await SynonymCommentProvider.GetSynonymComments("synonym_comment_synonym_1").IsSome.ConfigureAwait(false);
            Assert.That(synonymIsSome, Is.True);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            const string synonymName = "synonym_comment_synonym_1";
            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName.LocalName, Is.EqualTo(synonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName, Is.EqualTo(synonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "synonym_comment_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_comment_synonym_1");

            var synonymComments = await SynonymCommentProvider.GetSynonymComments(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonymComments.SynonymName, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymMissing_ReturnsNone()
        {
            var synonymIsNone = await SynonymCommentProvider.GetSynonymComments("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(synonymIsNone, Is.True);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("SYNONYM_COMMENT_synonym_1");
            var synonymComments = await SynonymCommentProvider.GetSynonymComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonymComments.SynonymName.LocalName);
            Assert.That(equalNames, Is.True);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "SYNONYM_COMMENT_synonym_1");
            var synonymComments = await SynonymCommentProvider.GetSynonymComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonymComments.SynonymName.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonymComments.SynonymName.LocalName);
            Assert.That(equalNames, Is.True);
        }

        [Test]
        public async Task GetAllSynonymComments_WhenEnumerated_ContainsSynonymComments()
        {
            var hasSynonymComments = await SynonymCommentProvider.GetAllSynonymComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasSynonymComments, Is.True);
        }

        [Test]
        public async Task GetAllSynonymComments_WhenEnumerated_ContainsTestSynonymComment()
        {
            var containsTestSynonym = await SynonymCommentProvider.GetAllSynonymComments()
                .AnyAsync(t => string.Equals(t.SynonymName.LocalName, "synonym_comment_synonym_1", StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestSynonym, Is.True);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymMissingComment_ReturnsNone()
        {
            var comments = await GetSynonymCommentsAsync("synonym_comment_synonym_1").ConfigureAwait(false);

            Assert.That(comments.Comment.IsNone, Is.True);
        }

        [Test]
        public async Task GetSynonymComments_WhenSynonymContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test synonym comment.";
            var comments = await GetSynonymCommentsAsync("synonym_comment_synonym_2").ConfigureAwait(false);

            var synonymComment = comments.Comment.UnwrapSome();

            Assert.That(synonymComment, Is.EqualTo(expectedComment));
        }
    }
}
