using System;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class ForeignKeyColumnTypeMismatchRuleTests
{
    private static DatabaseColumn CreateColumn(string name, string typeName, string typeDefinition, int maxLength = 0, string collation = null)
    {
        var dbType = new ColumnDataType(
            typeName,
            DataType.Unknown,
            typeDefinition,
            typeof(object),
            false,
            maxLength,
            Option<INumericPrecision>.None,
            collation == null ? Option<Identifier>.None : Option<Identifier>.Some(collation)
        );
        return new DatabaseColumn(name, dbType, true, null, null);
    }

    private static IRelationalDatabaseTable CreateChildTable(IDatabaseColumn childColumn, IDatabaseColumn parentColumn)
    {
        var childKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            [childColumn],
            true
        );
        var parentKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [parentColumn],
            true
        );
        var relationalKey = new DatabaseRelationalKey(
            "child_table",
            childKey,
            "parent_table",
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        return new RelationalDatabaseTable(
            "child_table",
            [childColumn],
            null,
            [],
            [relationalKey],
            [],
            [],
            [],
            []
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeyColumnTypeMismatchRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenMatchingColumnTypes_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("parent_id", "integer", "integer");
        var parentColumn = CreateColumn("id", "integer", "integer");
        var tables = new[] { CreateChildTable(childColumn, parentColumn) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenMismatchingColumnTypes_ProducesMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("parent_id", "integer", "integer");
        var parentColumn = CreateColumn("id", "bigint", "bigint");
        var tables = new[] { CreateChildTable(childColumn, parentColumn) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    // the same type written differently is the same type, e.g. when the two tables were described by
    // different catalog queries
    [Test]
    public static async Task AnalyseTables_GivenSameTypeWrittenDifferently_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("parent_name", "varchar", "varchar(50)", 50);
        var parentColumn = CreateColumn("name", "VARCHAR", "VARCHAR(50)", 50);
        var tables = new[] { CreateChildTable(childColumn, parentColumn) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenSameTypeNameWithDifferentLengths_ProducesMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("parent_name", "varchar", "varchar(50)", 50);
        var parentColumn = CreateColumn("name", "varchar", "varchar(100)", 100);
        var tables = new[] { CreateChildTable(childColumn, parentColumn) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    // a collation mismatch is reported by a rule of its own, so it is not also reported here
    [Test]
    public static async Task AnalyseTables_GivenColumnTypesDifferingOnlyInCollation_ProducesNoMessages()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("parent_name", "varchar", "varchar(50)", 50, "Latin1_General_CI_AS");
        var parentColumn = CreateColumn("name", "varchar", "varchar(50)", 50, "SQL_Latin1_General_CP1_CS_AS");
        var tables = new[] { CreateChildTable(childColumn, parentColumn) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenMismatchingColumnTypes_NamesForeignKeyAndTables()
    {
        var rule = new ForeignKeyColumnTypeMismatchRule(RuleLevel.Error);
        var childColumn = CreateColumn("parent_id", "integer", "integer");
        var parentColumn = CreateColumn("id", "bigint", "bigint");
        var tables = new[] { CreateChildTable(childColumn, parentColumn) };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(
            messages,
            Has.One.Matches<IRuleMessage>(static m =>
                m.Message.Contains("'test_foreign_key'", StringComparison.Ordinal)
                && m.Message.Contains("child_table", StringComparison.Ordinal)
                && m.Message.Contains("parent_table", StringComparison.Ordinal))
        );
    }
}
