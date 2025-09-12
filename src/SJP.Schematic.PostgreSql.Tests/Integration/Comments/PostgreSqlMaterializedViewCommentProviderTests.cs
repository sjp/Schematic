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

namespace SJP.Schematic.PostgreSql.Tests.Integration.Comments;

internal sealed class PostgreSqlMaterializedViewCommentProviderTests : PostgreSqlTest
{
    private IDatabaseViewCommentProvider ViewCommentProvider => new PostgreSqlMaterializedViewCommentProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table matview_comment_table_1 (test_column_1 int primary key not null, test_column_2 int, test_column_3 int)", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create materialized view matview_comment_matview_1 as select test_column_1, test_column_2, test_column_3 from matview_comment_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create materialized view matview_comment_matview_2 as select test_column_1, test_column_2, test_column_3 from matview_comment_table_1", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("comment on materialized view matview_comment_matview_2 is 'This is a test materialized view.'", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("comment on column matview_comment_matview_2.test_column_2 is 'This is a test materialized view column.'", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop materialized view matview_comment_matview_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop materialized view matview_comment_matview_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table matview_comment_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IDatabaseViewComments> GetViewCommentsAsync(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

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

            return await lazyComment.ConfigureAwait(false);
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseViewComments>> _commentsCache = [];

    [Test]
    public async Task GetViewComments_WhenViewPresent_ReturnsViewComment()
    {
        var viewIsSome = await ViewCommentProvider.GetViewComments("matview_comment_matview_1").IsSome.ConfigureAwait(false);
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        const string viewName = "matview_comment_matview_1";
        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName.LocalName, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("matview_comment_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "matview_comment_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "matview_comment_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenDifferenceCaseName_ShouldBeResolvedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MATVIEW_COMMENT_MATVIEW_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_comment_matview_1");

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
            .AnyAsync(t => string.Equals(t.ViewName.LocalName, "matview_comment_matview_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewMissingComment_ReturnsNone()
    {
        var comments = await GetViewCommentsAsync("matview_comment_matview_1").ConfigureAwait(false);

        Assert.That(comments.Comment.IsNone, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
    {
        var columnNames = new[]
        {
            new Identifier("test_column_1"),
            new Identifier("test_column_2"),
            new Identifier("test_column_3"),
        };

        var comments = await GetViewCommentsAsync("matview_comment_matview_1").ConfigureAwait(false);

        Assert.That(comments.ColumnComments.Keys.Order(), Is.EqualTo(columnNames));
    }

    [Test]
    public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
    {
        var comments = await GetViewCommentsAsync("matview_comment_matview_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasOnlyNones = columnComments.All(c => c.Value.IsNone);

        Assert.That(hasOnlyNones, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test materialized view.";
        var comments = await GetViewCommentsAsync("matview_comment_matview_2").ConfigureAwait(false);

        var viewComment = comments.Comment.UnwrapSome();

        Assert.That(viewComment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetViewComments_PropertyGetForColumnComments_ReturnsAllColumnNamesAsKeys()
    {
        var columnNames = new[]
        {
            new Identifier("test_column_1"),
            new Identifier("test_column_2"),
            new Identifier("test_column_3"),
        };
        var comments = await GetViewCommentsAsync("matview_comment_matview_2").ConfigureAwait(false);

        Assert.That(comments.ColumnComments.Keys.Order(), Is.EqualTo(columnNames));
    }

    [Test]
    public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[]
        {
            true,
            false,
            true,
        };
        var comments = await GetViewCommentsAsync("matview_comment_matview_2").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var noneStates = new[]
        {
            columnComments["test_column_1"].IsNone,
            columnComments["test_column_2"].IsNone,
            columnComments["test_column_3"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a test materialized view column.";
        var comments = await GetViewCommentsAsync("matview_comment_matview_2").ConfigureAwait(false);

        var comment = comments.ColumnComments["test_column_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }
}