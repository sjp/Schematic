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
    internal sealed class OracleViewCommentProviderTests : OracleTest
    {
        private IDatabaseViewCommentProvider ViewCommentProvider => new OracleViewCommentProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table wrapper_view_comment_table_1 ( test_column_1 number, test_column_2 number, test_column_3 number )").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view wrapper_view_comment_view_1 as select test_column_1, test_column_2, test_column_3 from wrapper_view_comment_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view wrapper_view_comment_view_2 as select test_column_1, test_column_2, test_column_3 from wrapper_view_comment_table_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("create materialized view wrapper_view_comment_mview_1 as select test_column_1, test_column_2, test_column_3 from wrapper_view_comment_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create materialized view wrapper_view_comment_mview_2 as select test_column_1, test_column_2, test_column_3 from wrapper_view_comment_table_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("comment on table wrapper_view_comment_view_2 is 'This is a test view comment.'").ConfigureAwait(false);
            await Connection.ExecuteAsync("comment on column wrapper_view_comment_view_2.test_column_2 is 'This is a column comment.'").ConfigureAwait(false);
            await Connection.ExecuteAsync("comment on materialized view wrapper_view_comment_mview_2 is 'This is a test materialized view comment.'").ConfigureAwait(false);
            await Connection.ExecuteAsync("comment on column wrapper_view_comment_mview_2.test_column_2 is 'This is an mview column comment.'").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view wrapper_view_comment_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view wrapper_view_comment_view_2").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop materialized view wrapper_view_comment_mview_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop materialized view wrapper_view_comment_mview_2").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop table wrapper_view_comment_table_1").ConfigureAwait(false);
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
            var viewIsSome = await ViewCommentProvider.GetViewComments("WRAPPER_VIEW_COMMENT_VIEW_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresent_ReturnsViewCommentWithCorrectName()
        {
            const string viewName = "WRAPPER_VIEW_COMMENT_VIEW_1";
            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, viewComments.ViewName.LocalName);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("wrapper_view_comment_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_VIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "wrapper_view_comment_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_VIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "wrapper_view_comment_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_VIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_VIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "wrapper_view_comment_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_VIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "wrapper_view_comment_view_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_VIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenViewMissing_ReturnsNone()
        {
            var viewIsNone = await ViewCommentProvider.GetViewComments("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(viewIsNone);
        }

        [Test]
        public async Task GetAllViewComments_WhenEnumerated_ContainsViewComments()
        {
            var hasViewComments = await ViewCommentProvider.GetAllViewComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsTrue(hasViewComments);
        }

        [Test]
        public async Task GetAllViewComments_WhenEnumerated_ContainsTestViewComment()
        {
            var containsTestView = await ViewCommentProvider.GetAllViewComments()
                .AnyAsync(v => v.ViewName.LocalName == "WRAPPER_VIEW_COMMENT_VIEW_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestView);
        }

        [Test]
        public async Task GetViewComments_WhenViewMissingComment_ReturnsNone()
        {
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
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

            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var allKeysPresent = columnNames.All(columnComments.ContainsKey);

            Assert.IsTrue(allKeysPresent);
        }

        [Test]
        public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
        {
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var hasOnlyNones = columnComments.All(c => c.Value.IsNone);

            Assert.IsTrue(hasOnlyNones);
        }

        [Test]
        public async Task GetViewComments_WhenViewContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test view comment.";
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_2").ConfigureAwait(false);

            var viewComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, viewComment);
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
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var allKeysPresent = columnNames.All(columnComments.ContainsKey);

            Assert.IsTrue(allKeysPresent);
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
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var noneStates = new[]
            {
                columnComments["TEST_COLUMN_1"].IsNone,
                columnComments["TEST_COLUMN_2"].IsNone,
                columnComments["TEST_COLUMN_3"].IsNone
            };

            var seqEqual = expectedNoneStates.SequenceEqual(noneStates);

            Assert.IsTrue(seqEqual);
        }

        [Test]
        public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
        {
            const string expectedComment = "This is a column comment.";
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_VIEW_2").ConfigureAwait(false);

            var comment = comments.ColumnComments["TEST_COLUMN_2"].UnwrapSome();

            Assert.AreEqual(expectedComment, comment);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresent_ReturnsMatViewComment()
        {
            var viewIsSome = await ViewCommentProvider.GetViewComments("wrapper_view_comment_mview_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(viewIsSome);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresent_ReturnsMatViewCommentWithCorrectName()
        {
            const string viewName = "WRAPPER_VIEW_COMMENT_MVIEW_1";
            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, viewComments.ViewName.LocalName);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("wrapper_view_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Schema, "wrapper_view_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "wrapper_view_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(viewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "wrapper_view_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "wrapper_view_comment_mview_1");
            var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "WRAPPER_VIEW_COMMENT_MVIEW_1");

            var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedViewName, viewComments.ViewName);
        }

        [Test]
        public async Task GetAllViewComments_WhenEnumerated_ContainsTestMatViewComment()
        {
            var containsTestView = await ViewCommentProvider.GetAllViewComments()
                .AnyAsync(v => v.ViewName.LocalName == "WRAPPER_VIEW_COMMENT_MVIEW_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestView);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewMissingComment_ReturnsDefaultComment()
        {
            var expectedComment = $"snapshot table for snapshot {IdentifierDefaults.Schema}.WRAPPER_VIEW_COMMENT_MVIEW_1";

            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_1").ConfigureAwait(false);
            var comment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, comment);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
        {
            var columnNames = new[]
            {
                new Identifier("TEST_COLUMN_1"),
                new Identifier("TEST_COLUMN_2"),
                new Identifier("TEST_COLUMN_3")
            };

            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var allKeysPresent = columnNames.All(columnComments.ContainsKey);

            Assert.IsTrue(allKeysPresent);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
        {
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_1").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var hasOnlyNones = columnComments.All(c => c.Value.IsNone);

            Assert.IsTrue(hasOnlyNones);
        }

        [Test]
        public async Task GetViewComments_WhenMatViewContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test materialized view comment.";
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var viewComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, viewComment);
        }

        [Test]
        public async Task GetViewComments_PropertyGetForMatViewColumnComments_ReturnsAllColumnNamesAsKeys()
        {
            var columnNames = new[]
            {
                new Identifier("TEST_COLUMN_1"),
                new Identifier("TEST_COLUMN_2"),
                new Identifier("TEST_COLUMN_3")
            };
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var allKeysPresent = columnNames.All(columnComments.ContainsKey);

            Assert.IsTrue(allKeysPresent);
        }

        [Test]
        public async Task GetViewComments_PropertyGetForMatViewColumnComments_ReturnsCorrectNoneOrSome()
        {
            var expectedNoneStates = new[]
            {
                true,
                false,
                true
            };
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var columnComments = comments.ColumnComments;
            var noneStates = new[]
            {
                columnComments["TEST_COLUMN_1"].IsNone,
                columnComments["TEST_COLUMN_2"].IsNone,
                columnComments["TEST_COLUMN_3"].IsNone
            };

            var seqEqual = expectedNoneStates.SequenceEqual(noneStates);

            Assert.IsTrue(seqEqual);
        }

        [Test]
        public async Task GetViewComments_PropertyGetForMatViewColumnComments_ReturnsCorrectCommentValue()
        {
            const string expectedComment = "This is an mview column comment.";
            var comments = await GetViewCommentsAsync("WRAPPER_VIEW_COMMENT_MVIEW_2").ConfigureAwait(false);

            var comment = comments.ColumnComments["TEST_COLUMN_2"].UnwrapSome();

            Assert.AreEqual(expectedComment, comment);
        }
    }
}
