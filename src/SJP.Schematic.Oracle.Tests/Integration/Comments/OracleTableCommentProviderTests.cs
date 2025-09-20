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
using SJP.Schematic.Oracle.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration.Comments;

internal sealed class OracleTableCommentProviderTests : OracleTest
{
    private IRelationalDatabaseTableCommentProvider TableCommentProvider => new OracleTableCommentProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_comment_table_1 ( test_column_1 number )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_comment_table_2 ( test_column_1 number, test_column_2 number, test_column_3 number )", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("comment on table table_comment_table_2 is 'This is a test table comment.'", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("comment on column table_comment_table_2.test_column_2 is 'This is a column comment.'", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_comment_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_comment_table_2", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IRelationalDatabaseTableComments> GetTableCommentsAsync(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetTableCommentsAsyncCore(tableName);
    }

    private async Task<IRelationalDatabaseTableComments> GetTableCommentsAsyncCore(Identifier tableName)
    {
        using (await _lock.LockAsync().ConfigureAwait(false))
        {
            if (!_commentsCache.TryGetValue(tableName, out var lazyComment))
            {
                lazyComment = new AsyncLazy<IRelationalDatabaseTableComments>(() => TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync());
                _commentsCache[tableName] = lazyComment;
            }

            return await lazyComment.ConfigureAwait(false);
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTableComments>> _commentsCache = [];

    [Test]
    public async Task GetTableComments_WhenTablePresent_ReturnsTableComment()
    {
        var tableIsSome = await TableCommentProvider.GetTableComments("table_comment_table_1").IsSome.ConfigureAwait(false);
        Assert.That(tableIsSome, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTablePresent_ReturnsTableWithCorrectName()
    {
        const string tableName = "TABLE_COMMENT_TABLE_1";
        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName.LocalName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "TABLE_COMMENT_TABLE_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTableMissing_ReturnsNone()
    {
        var tableIsNone = await TableCommentProvider.GetTableComments("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllTableComments_WhenEnumerated_ContainsTableComments()
    {
        var hasTableComments = await TableCommentProvider.EnumerateAllTableComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasTableComments, Is.True);
    }

    [Test]
    public async Task EnumerateAllTableComments_WhenEnumerated_ContainsTestTableComment()
    {
        var containsTestTable = await TableCommentProvider.EnumerateAllTableComments()
            .AnyAsync(t => string.Equals(t.TableName.LocalName, "TABLE_COMMENT_TABLE_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetAllTableComments2_WhenRetrieved_ContainsTableComments()
    {
        var tableComments = await TableCommentProvider.GetAllTableComments2().ConfigureAwait(false);

        Assert.That(tableComments, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllTableComments2_WhenRetrieved_ContainsTestTableComment()
    {
        var tableComments = await TableCommentProvider.GetAllTableComments2().ConfigureAwait(false);
        var containsTestTable = tableComments.Any(t => string.Equals(t.TableName.LocalName, "TABLE_COMMENT_TABLE_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingComment_ReturnsNone()
    {
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_1").ConfigureAwait(false);

        Assert.That(comments.Comment.IsNone, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
    {
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasColumns = columnComments.Count == 1 && string.Equals(columnComments.Keys.Single().LocalName, "TEST_COLUMN_1", StringComparison.Ordinal);

        Assert.That(hasColumns, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
    {
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasOnlyNones = columnComments.Count == 1 && columnComments.Values.Single().IsNone;

        Assert.That(hasOnlyNones, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test table comment.";
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

        var tableComment = comments.Comment.UnwrapSome();

        Assert.That(tableComment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForColumnComments_ReturnsAllColumnNamesAsKeys()
    {
        var columnNames = new[]
        {
            new Identifier("TEST_COLUMN_1"),
            new Identifier("TEST_COLUMN_2"),
            new Identifier("TEST_COLUMN_3"),
        };
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

        Assert.That(comments.ColumnComments.Keys, Is.EqualTo(columnNames));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForColumnComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[]
        {
            true,
            false,
            true,
        };
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var noneStates = new[]
        {
            columnComments["TEST_COLUMN_1"].IsNone,
            columnComments["TEST_COLUMN_2"].IsNone,
            columnComments["TEST_COLUMN_3"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a column comment.";
        var comments = await GetTableCommentsAsync("TABLE_COMMENT_TABLE_2").ConfigureAwait(false);

        var comment = comments.ColumnComments["TEST_COLUMN_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }
}