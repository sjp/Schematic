using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments
{
    internal sealed class SqlServerSequenceCommentProviderTests : SqlServerTest
    {
        private IDatabaseSequenceCommentProvider SequenceCommentProvider => new SqlServerSequenceCommentProvider(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create sequence sequence_comment_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence sequence_comment_sequence_2").ConfigureAwait(false);

            await AddCommentForSequence("This is a test sequence comment.", "dbo", "sequence_comment_sequence_2").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop sequence sequence_comment_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence sequence_comment_sequence_2").ConfigureAwait(false);
        }

        private Task AddCommentForSequence(string comment, string schemaName, string sequenceName)
        {
            const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'SEQUENCE',
  @level1name = @SequenceName";
            return Connection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, SequenceName = sequenceName });
        }

        private Task<IDatabaseSequenceComments> GetSequenceCommentsAsync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            lock (_lock)
            {
                if (!_commentsCache.TryGetValue(sequenceName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseSequenceComments>(() => SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync());
                    _commentsCache[sequenceName] = lazyComment;
                }

                return lazyComment.GetValueAsync(CancellationToken.None);
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseSequenceComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseSequenceComments>>();

        [Test]
        public async Task GetSequenceComments_WhenSequencePresent_ReturnsSequenceComment()
        {
            var sequenceIsSome = await SequenceCommentProvider.GetSequenceComments("sequence_comment_sequence_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(sequenceIsSome);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresent_ReturnsSequenceWithCorrectName()
        {
            const string sequenceName = "sequence_comment_sequence_1";
            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(sequenceName, sequenceComments.SequenceName.LocalName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("sequence_comment_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequenceComments.SequenceName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Schema, "sequence_comment_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequenceComments.SequenceName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequenceComments.SequenceName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(sequenceName, sequenceComments.SequenceName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequenceComments.SequenceName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "sequence_comment_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "sequence_comment_sequence_1");

            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequenceComments.SequenceName);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequenceMissing_ReturnsNone()
        {
            var sequenceIsNone = await SequenceCommentProvider.GetSequenceComments("sequence_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(sequenceIsNone);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("SEQUENCE_COMMENT_sequence_1");
            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, sequenceComments.SequenceName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequencePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "SEQUENCE_COMMENT_sequence_1");
            var sequenceComments = await SequenceCommentProvider.GetSequenceComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, sequenceComments.SequenceName.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, sequenceComments.SequenceName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetAllSequenceComments_WhenEnumerated_ContainsSequenceComments()
        {
            var hasSequenceComments = await SequenceCommentProvider.GetAllSequenceComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsTrue(hasSequenceComments);
        }

        [Test]
        public async Task GetAllSequenceComments_WhenEnumerated_ContainsTestSequenceComment()
        {
            var containsTestSequence = await SequenceCommentProvider.GetAllSequenceComments()
                .AnyAsync(t => t.SequenceName.LocalName == "sequence_comment_sequence_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestSequence);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequenceMissingComment_ReturnsNone()
        {
            var comments = await GetSequenceCommentsAsync("sequence_comment_sequence_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetSequenceComments_WhenSequenceContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test sequence comment.";
            var comments = await GetSequenceCommentsAsync("sequence_comment_sequence_2").ConfigureAwait(false);

            var sequenceComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, sequenceComment);
        }
    }
}
