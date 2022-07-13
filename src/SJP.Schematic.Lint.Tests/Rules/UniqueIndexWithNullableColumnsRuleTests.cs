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
internal static class UniqueIndexWithNullableColumnsRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new UniqueIndexWithNullableColumnsRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoIndexes_ProducesNoMessages()
    {
        var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);

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
    public static async Task AnalyseTables_GivenTableWithNoUniqueIndexes_ProducesNoMessages()
    {
        var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column_1",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );

        var index = new DatabaseIndex(
            "test_index_name",
            false,
            new[] { new DatabaseIndexColumn("test_column_1", testColumn, IndexColumnOrder.Ascending) },
            Array.Empty<IDatabaseColumn>(),
            true,
            Option<string>.None
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            new[] { index },
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNoNullableColumnsInUniqueIndex_ProducesNoMessages()
    {
        var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column_1",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );

        var uniqueIndex = new DatabaseIndex(
            "test_index_name",
            true,
            new[] { new DatabaseIndexColumn("test_column_1", testColumn, IndexColumnOrder.Ascending) },
            Array.Empty<IDatabaseColumn>(),
            true,
            Option<string>.None
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            new[] { uniqueIndex },
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithNullableColumnsInUniqueIndex_ProducesMessages()
    {
        var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column_1",
            Mock.Of<IDbType>(),
            true,
            null,
            null
        );

        var uniqueIndex = new DatabaseIndex(
            "test_index_name",
            true,
            new[] { new DatabaseIndexColumn("test_column_1", testColumn, IndexColumnOrder.Ascending) },
            Array.Empty<IDatabaseColumn>(),
            true,
            Option<string>.None
        );

        var table = new RelationalDatabaseTable(
            "test",
            new List<IDatabaseColumn>(),
            null,
            Array.Empty<IDatabaseKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            Array.Empty<IDatabaseRelationalKey>(),
            new[] { uniqueIndex },
            Array.Empty<IDatabaseCheckConstraint>(),
            Array.Empty<IDatabaseTrigger>()
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}