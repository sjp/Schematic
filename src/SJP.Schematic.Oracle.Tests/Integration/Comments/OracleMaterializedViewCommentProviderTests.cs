using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Comments;

namespace SJP.Schematic.Oracle.Tests.Integration.Comments
{
    internal sealed class OracleMaterializedViewCommentProviderTests : OracleTest
    {
        private IDatabaseViewCommentProvider ViewCommentProvider => new OracleMaterializedViewCommentProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table mview_comment_table_1 ( test_column_1 number, test_column_2 number, test_column_3 number )").ConfigureAwait(false);

            await Connection.ExecuteAsync("create materialized view mview_comment_mview_1 as select test_column_1, test_column_2, test_column_3 from mview_comment_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create materialized view mview_comment_mview_2 as select test_column_1, test_column_2, test_column_3 from mview_comment_table_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("comment on materialized view mview_comment_mview_2 is 'This is a test view comment.'").ConfigureAwait(false);
            await Connection.ExecuteAsync("comment on column mview_comment_mview_2.test_column_2 is 'This is a column comment.'").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop materialized view mview_comment_mview_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop materialized view mview_comment_mview_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table mview_comment_table_1").ConfigureAwait(false);
        }

        private Task<IDatabaseViewComments> GetViewCommentsAsync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetViewCommentsAsyncCore(viewName);
        }

        private async Task<IDatabaseViewComments> GetViewCommentsAsyncCore(Identifier viewName)
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
            {
                if (!_commentsCache.TryGetValue(viewName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseViewComments>(() => ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync());
                    _commentsCache[viewName] = lazyComment;
                }

                return await lazyComment;
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseViewComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseViewComments>>();

        [Test]
        public async Task GetViewComments_WhenViewPresent_ReturnsViewComment()
        {
            var viewIsSome = await ViewCommentProvider.GetViewComments("mview_comment_mview_1").IsSome.ConfigureAwait(false);
            Assert.That(viewIsSome, Is.True);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresent_ReturnsViewWithCorrectName()
        {
            const string viewName = "MVIEW_COMMENT_MVIEW_1";
            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName.LocalName, Is.EqualTo(viewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("mview_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "mview_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "mview_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName, Is.EqualTo(viewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "mview_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "mview_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
        }

        [Test]
        public async Task GetViewComments_WhenViewMissing_ReturnsNone()
        {
            var viewIsNone = await ViewCommentProvider.GetViewComments("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(viewIsNone, Is.True);
        }

        [Test]
        public async Task GetAllViewComments_WhenEnumerated_ContainsViewComments()
        {
            var hasViewComments = await ViewCommentProvider.GetAllViewComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasViewComments, Is.True);
        }

        [Test]
        public async Task GetAllViewComments_WhenEnumerated_ContainsTestViewComment()
        {
            var containsTestView = await ViewCommentProvider.GetAllViewComments()
                .AnyAsync(t => t.ViewName.LocalName == "MVIEW_COMMENT_MVIEW_1")
                .ConfigureAwait(false);

            Assert.That(containsTestView, Is.True);
        }

        [Test]
        public async Task GetViewComments_WhenViewMissingComment_ReturnsDefaultComment()
        {
            var expectedComment = $"snapshot table for snapshot {IdentifierDefaults.Schema}.MVIEW_COMMENT_MVIEW_1";

            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_1").ConfigureAwait(false);
            var comment = comments.Comment.UnwrapSome();

            Assert.That(comment, Is.EqualTo(expectedComment));
        }

        [Test]
        public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
        {
            var columnNames = new[]
            {
                new Identifier("TEST_COLUMN_1"),
                new Identifier("TEST_COLUMN_2"),
                new Identifier("TEST_COLUMN_3")
            };

            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_1").ConfigureAwait(false);

            Assert.That(comments.ColumnComments.Keys, Is.EqualTo(columnNames));
        }

        [Test]
        public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
        {
            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var hasOnlyNones = columnComments.All(c => c.Value.IsNone);

            Assert.That(hasOnlyNones, Is.True);
        }

        [Test]
        public async Task GetViewComments_WhenViewContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test view comment.";
            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var viewComment = comments.Comment.UnwrapSome();

            Assert.That(viewComment, Is.EqualTo(expectedComment));
        }

        [Test]
        public async Task GetViewComments_PropertyGetForColumnComments_ReturnsAllColumnNamesAsKeys()
        {
            var columnNames = new[]
            {
                new Identifier("TEST_COLUMN_1"),
                new Identifier("TEST_COLUMN_2"),
                new Identifier("TEST_COLUMN_3")
            };
            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            Assert.That(comments.ColumnComments.Keys, Is.EqualTo(columnNames));
        }

        [Test]
        public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectNoneOrSome()
        {
            var expectedNoneStates = new[]
            {
                true,
                false,
                true
            };
            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var noneStates = new[]
            {
                columnComments["TEST_COLUMN_1"].IsNone,
                columnComments["TEST_COLUMN_2"].IsNone,
                columnComments["TEST_COLUMN_3"].IsNone
            };

            Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
        }

        [Test]
        public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
        {
            const string expectedComment = "This is a column comment.";
            var comments = await GetViewCommentsAsync("MVIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var comment = comments.ColumnComments["TEST_COLUMN_2"].UnwrapSome();

            Assert.That(comment, Is.EqualTo(expectedComment));
        }
    }
}
