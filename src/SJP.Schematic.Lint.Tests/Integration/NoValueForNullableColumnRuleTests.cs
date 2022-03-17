using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class NoValueForNullableColumnRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_without_nullable_columns_1 ( column_1 integer not null )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_for_nullable_columns_1 ( column_1 integer not null, column_2 integer null )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table table_for_nullable_columns_2 ( column_1 integer not null, column_2 integer null )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("insert into table_for_nullable_columns_2 ( column_1 ) values (1)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_without_nullable_columns_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_for_nullable_columns_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_for_nullable_columns_2", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        ISchematicConnection connection = null;
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new NoValueForNullableColumnRule(connection, level), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NoValueForNullableColumnRule(connection, level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var rule = new NoValueForNullableColumnRule(connection, RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutNullableColumns_ProducesNoMessages()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_without_nullable_columns_1").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithNullableColumnsButNoRows_ProducesNoMessages()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_1").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithNullableColumnsWithNoData_ProducesMessages()
    {
        var rule = new NoValueForNullableColumnRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_for_nullable_columns_2").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}
