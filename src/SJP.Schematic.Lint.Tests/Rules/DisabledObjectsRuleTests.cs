using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class DisabledObjectsRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new DisabledObjectsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoDisabledObjects_ProducesNoMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseIndex>(),
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledPrimaryKey_ProducesMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            new[] { testColumn },
            false
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            testPrimaryKey,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseIndex>(),
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledForeignKey_ProducesMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testForeignKey = new DatabaseKey(
            Option<Identifier>.Some("test_foreign_key"),
            DatabaseKeyType.Foreign,
            new[] { testColumn },
            false
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            new[] { testColumn },
            false
        );
        var testRelationalKey = new DatabaseRelationalKey(
            "child_table",
            testForeignKey,
            "parent_table",
            testPrimaryKey,
            ReferentialAction.Cascade,
            ReferentialAction.Cascade
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            new[] { testRelationalKey },
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseIndex>(),
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledUniqueKey_ProducesMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testUniqueKey = new DatabaseKey(
            Option<Identifier>.Some("test_unique_key"),
            DatabaseKeyType.Unique,
            new[] { testColumn },
            false
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            new[] { testUniqueKey },
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseIndex>(),
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledIndex_ProducesMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testIndex = new DatabaseIndex(
            "test_index",
            true,
            new[] { new DatabaseIndexColumn("test_column", testColumn, IndexColumnOrder.Ascending) },
            Array.Empty<IDatabaseColumn>(),
            false
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            new[] { testIndex },
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledCheck_ProducesMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testCheck = new DatabaseCheckConstraint(
            Option<Identifier>.Some("test_check"),
            "test_check_definition",
            false
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseIndex>(),
            new[] { testCheck },
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithDisabledTrigger_ProducesMessages()
    {
        var rule = new DisabledObjectsRule(RuleLevel.Error);

        var testTrigger = new DatabaseTrigger(
            "test_check",
            "test_check_definition",
            TriggerQueryTiming.After,
            TriggerEvent.Insert,
            false
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseIndex>(),
            Array.Empty<IDatabaseCheckConstraint>(),
            new[] { testTrigger }
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}
