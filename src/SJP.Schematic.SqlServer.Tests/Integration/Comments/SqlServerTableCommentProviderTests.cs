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

internal sealed class SqlServerTableCommentProviderTests : SqlServerTest
{
    private IRelationalDatabaseTableCommentProvider TableCommentProvider => new SqlServerTableCommentProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_comment_table_1 ( test_column_1 int )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_comment_table_2 ( test_column_1 int, constraint test_comment_table_2_pk primary key (test_column_1) )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
CREATE TABLE table_comment_table_3
(
    test_column_1 INT,
    test_column_2 INT,
    test_column_3 INT,
    CONSTRAINT table_comment_table_3_pk PRIMARY KEY (test_column_1),
    CONSTRAINT table_comment_table_3_uk_1 UNIQUE (test_column_1, test_column_3),
    CONSTRAINT table_comment_table_3_uk_2 UNIQUE (test_column_2, test_column_3),
    CONSTRAINT table_comment_table_3_ck_1 CHECK (test_column_1 > 1),
    CONSTRAINT table_comment_table_3_ck_2 CHECK (test_column_1 < 1000),
    CONSTRAINT table_comment_table_3_fk_1 FOREIGN KEY (test_column_3) REFERENCES table_comment_table_3 (test_column_1),
    CONSTRAINT table_comment_table_3_fk_2 FOREIGN KEY (test_column_2) REFERENCES table_comment_table_3 (test_column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index table_comment_table_3_ix_1 on table_comment_table_3 (test_column_2)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create index table_comment_table_3_ix_2 on table_comment_table_3 (test_column_3)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger table_comment_table_3_trigger_1
on table_comment_table_3
for insert
as
begin
    declare @test int
end
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create trigger table_comment_table_3_trigger_2
on table_comment_table_3
for delete
as
begin
    declare @test int
end
", CancellationToken.None).ConfigureAwait(false);

        await AddCommentForTable("This is a test table comment.", "dbo", "table_comment_table_3").ConfigureAwait(false);
        await AddCommentForTableObject("This is a column comment.", "dbo", "table_comment_table_3", "COLUMN", "test_column_2").ConfigureAwait(false);
        await AddCommentForTableObject("This is a primary key comment.", "dbo", "table_comment_table_3", "CONSTRAINT", "table_comment_table_3_pk").ConfigureAwait(false);
        await AddCommentForTableObject("This is a unique key comment.", "dbo", "table_comment_table_3", "CONSTRAINT", "table_comment_table_3_uk_2").ConfigureAwait(false);
        await AddCommentForTableObject("This is a check comment.", "dbo", "table_comment_table_3", "CONSTRAINT", "table_comment_table_3_ck_2").ConfigureAwait(false);
        await AddCommentForTableObject("This is a foreign key comment.", "dbo", "table_comment_table_3", "CONSTRAINT", "table_comment_table_3_fk_2").ConfigureAwait(false);
        await AddCommentForTableObject("This is an index comment.", "dbo", "table_comment_table_3", "INDEX", "table_comment_table_3_ix_2").ConfigureAwait(false);
        await AddCommentForTableObject("This is a trigger comment.", "dbo", "table_comment_table_3", "TRIGGER", "table_comment_table_3_trigger_2").ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_comment_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_comment_table_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_comment_table_3", CancellationToken.None).ConfigureAwait(false);
    }

    private Task AddCommentForTable(string comment, string schemaName, string tableName)
    {
        const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'TABLE',
  @level1name = @TableName";
        return DbConnection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, TableName = tableName }, CancellationToken.None);
    }

    private Task AddCommentForTableObject(string comment, string schemaName, string tableName, string objectType, string objectName)
    {
        const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'TABLE',
  @level1name = @TableName,
  @level2type = @ObjectType,
  @level2name = @ObjectName";

        return DbConnection.ExecuteAsync(
            querySql,
            new
            {
                Comment = comment,
                SchemaName = schemaName,
                TableName = tableName,
                ObjectType = objectType,
                ObjectName = objectName,
            },
            CancellationToken.None
        );
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
        const string tableName = "table_comment_table_1";
        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName.LocalName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(tableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

        var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(tableComments.TableName, Is.EqualTo(expectedTableName));
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "table_comment_table_1");
        var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

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
    public async Task GetTableComments_WhenTablePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("TABLE_COMMENT_table_1");
        var tableComments = await TableCommentProvider.GetTableComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, tableComments.TableName.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Dbo", "TABLE_COMMENT_table_1");
        var tableComments = await TableCommentProvider.GetTableComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, tableComments.TableName.Schema)
            && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, tableComments.TableName.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetAllTableComments_WhenEnumerated_ContainsTableComments()
    {
        var hasTableComments = await TableCommentProvider.GetAllTableComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasTableComments, Is.True);
    }

    [Test]
    public async Task GetAllTableComments_WhenEnumerated_ContainsTestTableComment()
    {
        var containsTestTable = await TableCommentProvider.GetAllTableComments()
            .AnyAsync(t => string.Equals(t.TableName.LocalName, "table_comment_table_1", StringComparison.Ordinal))
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
        var containsTestTable = tableComments.Any(t => string.Equals(t.TableName.LocalName, "table_comment_table_1", StringComparison.Ordinal));

        Assert.That(containsTestTable, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingComment_ReturnsNone()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.Comment.IsNone, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingPrimaryKey_ReturnsNone()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.PrimaryKeyComment, OptionIs.None);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupKeyedWithColumnNames()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasColumns = columnComments.Count == 1 && string.Equals(columnComments.Keys.Single().LocalName, "test_column_1", StringComparison.Ordinal);

        Assert.That(hasColumns, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingColumnComments_ReturnsLookupWithOnlyNoneValues()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        var columnComments = comments.ColumnComments;
        var hasOnlyNones = columnComments.Count == 1 && columnComments.Values.Single().IsNone;

        Assert.That(hasOnlyNones, Is.True);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingChecks_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.CheckComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingUniqueKeys_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.UniqueKeyComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingIndexes_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.IndexComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingTriggers_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.TriggerComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableMissingForeignKeys_ReturnsEmptyLookup()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_1").ConfigureAwait(false);

        Assert.That(comments.ForeignKeyComments, Is.Empty);
    }

    [Test]
    public async Task GetTableComments_WhenTableContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test table comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var tableComment = comments.Comment.UnwrapSome();

        Assert.That(tableComment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_WhenTableContainsPrimaryKeyWithNoComment_ReturnsNone()
    {
        var comments = await GetTableCommentsAsync("table_comment_table_2").ConfigureAwait(false);

        Assert.That(comments.PrimaryKeyComment, OptionIs.None);
    }

    [Test]
    public async Task GetTableComments_WhenTableContainsPrimaryKeyWithComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a primary key comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var pkComment = comments.PrimaryKeyComment.UnwrapSome();

        Assert.That(pkComment, Is.EqualTo(expectedComment));
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
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

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
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

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
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var comment = comments.ColumnComments["test_column_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForIndexComments_ReturnsAllIndexNamesAsKeys()
    {
        var indexNames = new[]
        {
            new Identifier("table_comment_table_3_ix_1"),
            new Identifier("table_comment_table_3_ix_2"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        Assert.That(comments.IndexComments.Keys, Is.EqualTo(indexNames));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForIndexComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[] { true, false };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var indexComments = comments.IndexComments;
        var noneStates = new[]
        {
            indexComments["table_comment_table_3_ix_1"].IsNone,
            indexComments["table_comment_table_3_ix_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForIndexComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is an index comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var comment = comments.IndexComments["table_comment_table_3_ix_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForTriggerComments_ReturnsAllTriggerNamesAsKeys()
    {
        var triggerNames = new[]
        {
            new Identifier("table_comment_table_3_trigger_1"),
            new Identifier("table_comment_table_3_trigger_2"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        Assert.That(comments.TriggerComments.Keys, Is.EqualTo(triggerNames));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForTriggerComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[] { true, false };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var triggerComments = comments.TriggerComments;
        var noneStates = new[]
        {
            triggerComments["table_comment_table_3_trigger_1"].IsNone,
            triggerComments["table_comment_table_3_trigger_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForTriggerComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a trigger comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var comment = comments.TriggerComments["table_comment_table_3_trigger_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForCheckComments_ReturnsAllCheckNamesAsKeys()
    {
        var checkNames = new[]
        {
            new Identifier("table_comment_table_3_ck_1"),
            new Identifier("table_comment_table_3_ck_2"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        Assert.That(comments.CheckComments.Keys, Is.EqualTo(checkNames));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForCheckComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[] { true, false };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var checkComments = comments.CheckComments;
        var noneStates = new[]
        {
            checkComments["table_comment_table_3_ck_1"].IsNone,
            checkComments["table_comment_table_3_ck_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForCheckComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a check comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var comment = comments.CheckComments["table_comment_table_3_ck_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForUniqueKeyComments_ReturnsAllUniqueKeyNamesAsKeys()
    {
        var uniqueKeyNames = new[]
        {
            new Identifier("table_comment_table_3_uk_1"),
            new Identifier("table_comment_table_3_uk_2"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        Assert.That(comments.UniqueKeyComments.Keys, Is.EqualTo(uniqueKeyNames));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForUniqueKeyComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[] { true, false };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var uniqueKeyComments = comments.UniqueKeyComments;
        var noneStates = new[]
        {
            uniqueKeyComments["table_comment_table_3_uk_1"].IsNone,
            uniqueKeyComments["table_comment_table_3_uk_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForUniqueKeyComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a unique key comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var comment = comments.UniqueKeyComments["table_comment_table_3_uk_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForForeignKeyComments_ReturnsAllForeignKeyNamesAsKeys()
    {
        var foreignKeyNames = new[]
        {
            new Identifier("table_comment_table_3_fk_1"),
            new Identifier("table_comment_table_3_fk_2"),
        };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        Assert.That(comments.ForeignKeyComments.Keys, Is.EqualTo(foreignKeyNames));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForForeignKeyComments_ReturnsCorrectNoneOrSome()
    {
        var expectedNoneStates = new[] { true, false };
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var foreignKeyComments = comments.ForeignKeyComments;
        var noneStates = new[]
        {
            foreignKeyComments["table_comment_table_3_fk_1"].IsNone,
            foreignKeyComments["table_comment_table_3_fk_2"].IsNone,
        };

        Assert.That(noneStates, Is.EqualTo(expectedNoneStates));
    }

    [Test]
    public async Task GetTableComments_PropertyGetForForeignKeyComments_ReturnsCorrectCommentValue()
    {
        const string expectedComment = "This is a foreign key comment.";
        var comments = await GetTableCommentsAsync("table_comment_table_3").ConfigureAwait(false);

        var comment = comments.ForeignKeyComments["table_comment_table_3_fk_2"].UnwrapSome();

        Assert.That(comment, Is.EqualTo(expectedComment));
    }
}