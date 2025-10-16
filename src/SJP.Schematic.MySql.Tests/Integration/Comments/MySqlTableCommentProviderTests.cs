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
using SJP.Schematic.MySql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration.Comments;

internal sealed class MySqlTableCommentProviderTests : MySqlTest
{
    private IRelationalDatabaseTableCommentProvider TableCommentProvider => new MySqlTableCommentProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_comment_table_1 ( test_column_1 int )", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"
CREATE TABLE table_comment_table_2
(
    test_column_1 INT,
    test_column_2 INT COMMENT 'This is a column comment.',
    test_column_3 INT
) COMMENT 'This is a test table comment.'", CancellationToken.None);
        await DbConnection.ExecuteAsync("create index table_comment_table_2_ix_1 on table_comment_table_2 (test_column_2)", CancellationToken.None);
        await DbConnection.ExecuteAsync("create index table_comment_table_2_ix_2 on table_comment_table_2 (test_column_3) COMMENT 'This is an index comment.'", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_comment_table_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_comment_table_2", CancellationToken.None);
    }

    private Task<IRelationalDatabaseTableComments> GetTableCommentsAsync(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetTableCommentsAsyncCore(tableName);
    }

    private async Task<IRelationalDatabaseTableComments> GetTableCommentsAsyncCore(Identifier tableName)
    {
        using (await _lock.LockAsync())
        {
            if (!_commentsCache.TryGetValue(tableName, out var lazyComment))
            {
                lazyComment = new AsyncLazy<IRelationalDatabaseTableComments>(() => TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync());
                _commentsCache[tableName] = lazyComment;
            }

            return await lazyComment;
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IRelationalDatabaseTableComments>> _commentsCache = [];

    [Test]
    public async Task GetTableComments_WhenTablePresent_ReturnsTableComment()
    {
        var tableIsSome = await TableCommentProvider.GetTableComments("table_comment_table_1").IsSome;
        Assert.That(tableIsSome, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTablePresent_ReturnsTableWithCorrectName()
    {
        const string tableName = "table_comment_table_1";
        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName.LocalName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync();

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTableMissing_ReturnsNone()
    {
        var tableIsNone = await TableCommentProvider.GetTableComments("table_that_doesnt_exist").IsNone;
        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllTableComments_WhenEnumerated_ContainsTableComments()
    {
        var hasTableComments = await TableCommentProvider.EnumerateAllTableComments().AnyAsync();

        Assert.That(hasTableComments, Is.True);
    }

    [Test]
    public async Task EnumerateAllTableComments_WhenEnumerated_ContainsTestTableComment()
    {
        var containsTestTable = await TableCommentProvider.EnumerateAllTableComments()
            .AnyAsync(t => string.Equals(t.TableName.LocalName, "table_comment_table_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetAllTableComments_WhenRetrieved_ContainsTableComments()
    {
        var tableComments = await TableCommentProvider.GetAllTableComments();

        var hasComments = tableComments.Any(tc => tc.Comment.IsSome);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableComments, Is.Not.Empty);
            Assert.That(hasComments, Is.True);
        }
    }

    [Test]
    public async Task GetAllTableComments_WhenRetrieved_ContainsTestTableComment()
    {
        var tableComments = await TableCommentProvider.GetAllTableComments();
        var containsTestTable = tableComments.Any(t => string.Equals(t.TableName.LocalName, "table_comment_table_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingComment_ReturnsNone()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.Comment.IsNone, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingPrimaryKey_ReturnsNone()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.PrimaryKeyComment, OptionIs.None);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        var columnComments = comments.ColumnComments;
        var hasColumns = columnComments.Count == 1 && string.Equals(columnComments.Keys.Single().LocalName, "test_column_1", StringComparison.Ordinal);

        Assert.That(hasColumns, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        var columnComments = comments.ColumnComments;
        var hasOnlyNones = columnComments.Count == 1 && columnComments.Values.Single().IsNone;

        Assert.That(hasOnlyNones, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingChecks_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.CheckComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingUniqueKeys_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.UniqueKeyComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingIndexes_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.IndexComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingTriggers_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.TriggerComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingForeignKeys_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1");

        Assert.That(comments.ForeignKeyComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test table comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_2");

        var tableComment = comments.Comment.UnwrapSome();

        Assert.That(tableComment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForColumnComments_ReturnsAllColumnNamesAsKeys()
    {
        var columnNames = new[]
        {
            new Identifier("test_column_1"),
            new Identifier("test_column_2"),
            new Identifier("test_column_3"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_2");

        var columnComments = comments.ColumnComments;
        var allKeysPresent = columnNames.All(columnComments.ContainsKey);

        Assert.That(allKeysPresent, Is.True);
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
        var comments = await GetTableCommentsAsync("table_comment_table_2");

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
    public async Task GetTableComments_PropertyGetForColumnComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a column comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_2");

        var comment = comments.ColumnComments["test_column_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForIndexComments_ReturnsAllIndexNamesAsKeys()
    {
        var indexNames = new[]
        {
            new Identifier("table_comment_table_2_ix_1"),
            new Identifier("table_comment_table_2_ix_2"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_2");

        var indexComments = comments.IndexComments;
        var allKeysPresent = indexNames.All(indexComments.ContainsKey);

        Assert.That(allKeysPresent, Is.True);
    }

    [Test]
    public async Task GetTableComments_PropertyGetForIndexComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[] { true, false };
        var comments = await GetTableCommentsAsync("table_comment_table_2");

        var indexComments = comments.IndexComments;
        var noneStates = new[]
        {
            indexComments["table_comment_table_2_ix_1"].IsNone,
            indexComments["table_comment_table_2_ix_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForIndexComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is an index comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_2");

        var comment = comments.IndexComments["table_comment_table_2_ix_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }
}