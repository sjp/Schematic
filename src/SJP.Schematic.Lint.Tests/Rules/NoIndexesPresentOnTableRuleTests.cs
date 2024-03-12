﻿using System;
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
internal static class NoIndexesPresentOnTableRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NoIndexesPresentOnTableRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithoutAnyIndexesOrCandidateKeys_ProducesMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

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

    [Test]
    public static async Task AnalyseTables_GivenTableWithOnlyPrimaryKey_ProducesNoMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<IDatabaseKey>.Some(Mock.Of<IDatabaseKey>()),
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOnlyUniqueKey_ProducesNoMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

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
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<IDatabaseKey>.None,
            new[] { testUniqueKey },
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithOnlyIndex_ProducesNoMessages()
    {
        var rule = new NoIndexesPresentOnTableRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            new[] { Mock.Of<IDatabaseIndex>() },
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }
}