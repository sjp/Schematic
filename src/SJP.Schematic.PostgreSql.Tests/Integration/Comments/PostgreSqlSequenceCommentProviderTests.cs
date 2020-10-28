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
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Comments
{
    internal sealed class PostgreSqlSequenceCommentProviderTests : PostgreSqlTest
    {
        private IDatabaseSequenceCommentProvider SequenceCommentProvider => new PostgreSqlSequenceCommentProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync("create sequence comment_test_sequence_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create sequence comment_test_sequence_2", CancellationToken.None).ConfigureAwait(false);

            await DbConnection.ExecuteAsync("comment on sequence comment_test_sequence_2 is 'This is a test sequence.'", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop sequence comment_test_sequence_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop sequence comment_test_sequence_2", CancellationToken.None).ConfigureAwait(false);
        }

        private Task<IDatabaseSequenceComments> GetSequenceCommentsAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return GetSequenceCommentsAsyncCore(sequenceName);
        }

        private async Task<IDatabaseSequenceComments> GetSequenceCommentsAsyncCore(Identifier sequenceName)
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
            {
                if (!_commentsCache.TryGetValue(sequenceName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseSequenceComments>(() => SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync());
                    _commentsCache[sequenceName] = lazyComment;
                }

                return await lazyComment.ConfigureAwait(false);
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseSequenceComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseSequenceComments>>();

        [Test]
        public async Task GetSequenceComments_WhenSequencePresent_ReturnsSequenceComment()
        {
            var sequenceIsSome = await SequenceCommentProvider.GetSequenceComments("comment_test_sequence_1").IsSome.ConfigureAwait(false);
            Assert.That(sequenceIsSome, Is.True);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresent_ReturnsSequenceWithCorrectName()
        {
            const string sequenceName = "comment_test_sequence_1";
            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName.LocalName, Is.EqualTo(sequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("comment_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(expectedSequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Schema, "comment_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(expectedSequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(expectedSequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(sequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(expectedSequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "comment_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(expectedSequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenDifferentCasedName_ShouldBeResolvedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "COMMENT_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(sequenceComments.SequenceName, Is.EqualTo(expectedSequenceName));
        }

        [Test]
        public async Task GetSequenceComments_WhenSequenceMissing_ReturnsNone()
        {
            var sequenceIsNone = await SequenceCommentProvider.GetSequenceComments("sequence_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(sequenceIsNone, Is.True);
        }

        [Test]
        public async Task GetAllSequenceComments_WhenEnumerated_ContainsSequenceComments()
        {
            var hasSequenceComments = await SequenceCommentProvider.GetAllSequenceComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasSequenceComments, Is.True);
        }

        [Test]
        public async Task GetAllSequenceComments_WhenEnumerated_ContainsTestSequenceComment()
        {
            var containsTestSequence = await SequenceCommentProvider.GetAllSequenceComments()
                .AnyAsync(t => string.Equals(t.SequenceName.LocalName, "comment_test_sequence_1", StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestSequence, Is.True);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequenceMissingComment_ReturnsNone()
        {
            var comments = await GetSequenceCommentsAsync("comment_test_sequence_1").ConfigureAwait(false);

            Assert.That(comments.Comment.IsNone, Is.True);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequenceContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test sequence.";
            var comments = await GetSequenceCommentsAsync("comment_test_sequence_2").ConfigureAwait(false);

            var sequenceComment = comments.Comment.UnwrapSome();

            Assert.That(sequenceComment, Is.EqualTo(expectedComment));
        }
    }
}
