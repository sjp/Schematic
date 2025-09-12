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

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments;

internal sealed class SqlServerViewCommentProviderTests : SqlServerTest
{
    private IDatabaseViewCommentProvider ViewCommentProvider => new SqlServerViewCommentProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view view_comment_view_1 as select 1 as test_column_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view view_comment_view_2 as select 1 as test_column_1, 'test' as test_column_2", CancellationToken.None).ConfigureAwait(false);

        await AddCommentForView("This is a test view comment.", "dbo", "view_comment_view_2").ConfigureAwait(false);
        await AddCommentForViewColumn("This is a column comment.", "dbo", "view_comment_view_2", "test_column_2").ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view view_comment_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view view_comment_view_2", CancellationToken.None).ConfigureAwait(false);
    }

    private Task AddCommentForView(string comment, string schemaName, string viewName)
    {
        const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'VIEW',
  @level1name = @ViewName";
        return DbConnection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, ViewName = viewName }, CancellationToken.None);
    }

    private Task AddCommentForViewColumn(string comment, string schemaName, string viewName, string columnName)
    {
        const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'VIEW',
  @level1name = @ViewName,
  @level2type = N'COLUMN',
  @level2name = @ColumnName";

        return DbConnection.ExecuteAsync(
            querySql,
            new
            {
                Comment = comment,
                SchemaName = schemaName,
                ViewName = viewName,
                ColumnName = columnName,
            },
            CancellationToken.None
        );
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
        var viewIsSome = await ViewCommentProvider.GetViewComments("view_comment_view_1").IsSome.ConfigureAwait(false);
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        const string viewName = "view_comment_view_1";
        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName.LocalName, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("view_comment_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_comment_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");

        var viewComments = await ViewCommentProvider.GetViewComments(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(viewComments.ViewName, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "view_comment_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "view_comment_view_1");

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
    public async Task GetViewComments_WhenViewPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("VIEW_COMMENT_view_1");
        var viewComments = await ViewCommentProvider.GetViewComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, viewComments.ViewName.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Dbo", "VIEW_COMMENT_view_1");
        var viewComments = await ViewCommentProvider.GetViewComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, viewComments.ViewName.Schema)
            && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, viewComments.ViewName.LocalName);
        Assert.That(equalNames, Is.True);
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
            .AnyAsync(t => string.Equals(t.ViewName.LocalName, "view_comment_view_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewMissingComment_ReturnsNone()
    {
        var comments = await GetViewCommentsAsync("view_comment_view_1").ConfigureAwait(false);

        Assert.That(comments.Comment.IsNone, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
    {
        var comments = await GetViewCommentsAsync("view_comment_view_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasColumns = columnComments.Count == 1 && string.Equals(columnComments.Keys.Single().LocalName, "test_column_1", StringComparison.Ordinal);

        Assert.That(hasColumns, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
    {
        var comments = await GetViewCommentsAsync("view_comment_view_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasOnlyNones = columnComments.Count == 1 && columnComments.Values.Single().IsNone;

        Assert.That(hasOnlyNones, Is.True);
    }

    [Test]
    public async Task GetViewComments_WhenViewContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test view comment.";
        var comments = await GetViewCommentsAsync("view_comment_view_2").ConfigureAwait(false);

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
        };
        var comments = await GetViewCommentsAsync("view_comment_view_2").ConfigureAwait(false);

        Assert.That(comments.ColumnComments.Keys, Is.EqualTo(columnNames));
    }

    [Test]
    public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[]
        {
            true,
            false,
        };
        var comments = await GetViewCommentsAsync("view_comment_view_2").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var noneStates = new[]
        {
            columnComments["test_column_1"].IsNone,
            columnComments["test_column_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetViewComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a column comment.";
        var comments = await GetViewCommentsAsync("view_comment_view_2").ConfigureAwait(false);

        var comment = comments.ColumnComments["test_column_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }
}