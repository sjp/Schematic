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
internal static class OrphanedTableRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new OrphanedTableRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithParentKeys_ProducesNoMessages()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);

        var parentKey = new DatabaseRelationalKey(
            "child_table",
            new DatabaseKey(
                Option<Identifier>.Some("child_key"),
                DatabaseKeyType.Foreign,
                new[] { Mock.Of<IDatabaseColumn>() },
                true
            ),
            "parent_table",
            new DatabaseKey(
                Option<Identifier>.Some("parent_key"),
                DatabaseKeyType.Primary,
                new[] { Mock.Of<IDatabaseColumn>() },
                true
            ),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var childTable = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            new[] { parentKey },
            [],
            [],
            [],
            []
        );
        var tables = new[] { childTable };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithChildKeys_ProducesNoMessages()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);

        var childKey = new DatabaseRelationalKey(
            "child_table",
            new DatabaseKey(
                Option<Identifier>.Some("child_key"),
                DatabaseKeyType.Foreign,
                new[] { Mock.Of<IDatabaseColumn>() },
                true
            ),
            "parent_table",
            new DatabaseKey(
                Option<Identifier>.Some("parent_key"),
                DatabaseKeyType.Primary,
                new[] { Mock.Of<IDatabaseColumn>() },
                true
            ),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var parentTable = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            new[] { childKey },
            [],
            [],
            []
        );
        var tables = new[] { parentTable };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoRelations_ProducesMessages()
    {
        var rule = new OrphanedTableRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}