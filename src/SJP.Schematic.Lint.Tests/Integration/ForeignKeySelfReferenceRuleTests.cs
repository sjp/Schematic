﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class ForeignKeySelfReferenceRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        // no primary key
        await DbConnection.ExecuteAsync(@"
create table table_without_self_referencing_columns_1 (
    column_1 integer,
    column_2 integer
)", CancellationToken.None).ConfigureAwait(false);

        // no self referencing foreign keys
        await DbConnection.ExecuteAsync(@"
create table table_without_self_referencing_columns_2_parent (
    column_1 integer not null primary key autoincrement,
    column_2 integer
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table table_without_self_referencing_columns_2 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint test_fk_1 foreign key (column_2) references table_without_self_referencing_columns_2_parent (column_1)
)", CancellationToken.None).ConfigureAwait(false);

        // no rows for same-row self-reference
        await DbConnection.ExecuteAsync(@"
create table table_without_self_referencing_columns_3 (
    column_1 integer not null primary key autoincrement,
    column_2 integer null,
    constraint self_ref_fk_1 foreign key (column_2) references table_without_self_referencing_columns_3 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("insert into table_without_self_referencing_columns_3 (column_1, column_2) values (1, NULL)", CancellationToken.None).ConfigureAwait(false);

        // rows containing a same-row self-reference present
        await DbConnection.ExecuteAsync(@"
create table table_with_self_referencing_columns_1 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint self_ref_fk_1 foreign key (column_2) references table_with_self_referencing_columns_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("insert into table_with_self_referencing_columns_1 (column_1, column_2) values (1, 1)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_without_self_referencing_columns_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_without_self_referencing_columns_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_without_self_referencing_columns_2_parent", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_without_self_referencing_columns_3", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table table_with_self_referencing_columns_1", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        ISchematicConnection connection = null;
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new ForeignKeySelfReferenceRule(connection, level), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeySelfReferenceRule(connection, level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var rule = new ForeignKeySelfReferenceRule(connection, RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithNoPrimaryKeys_ProducesNoMessages()
    {
        var rule = new ForeignKeySelfReferenceRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_without_self_referencing_columns_1").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithNoSelfReferencingForeignKeys_ProducesNoMessages()
    {
        var rule = new ForeignKeySelfReferenceRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_without_self_referencing_columns_2_parent").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("table_without_self_referencing_columns_2").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithNoRowsContainingSelfReferencesForSelfReferencingForeignKeys_ProducesNoMessages()
    {
        var rule = new ForeignKeySelfReferenceRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_without_self_referencing_columns_3").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithRowsContainingSelfReferencesForSelfReferencingForeignKeys_ProducesMessages()
    {
        var rule = new ForeignKeySelfReferenceRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_with_self_referencing_columns_1").UnwrapSomeAsync().ConfigureAwait(false)
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}