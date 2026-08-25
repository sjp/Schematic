using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class CycleDetectorTests
{
    [Test]
    public static void GetCyclePaths_GivenNullTables_ThrowsArgumentNullException()
    {
        var cycleDetector = new CycleDetector();

        Assert.That(() => cycleDetector.GetCyclePaths(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetCyclePaths_GivenEmptyTables_ReturnsEmptyCollection()
    {
        var cycleDetector = new CycleDetector();
        var result = cycleDetector.GetCyclePaths([]);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void GetCyclePaths_GivenTablesWithNoCycle_ReturnsEmptyCollection()
    {
        var cycleDetector = new CycleDetector();

        // create tables with no cycle where the path is a -> b -> c
        var tables = new[]
        {
            CreateTable("a", "b"),
            CreateTable("b", "c"),
            CreateTable("c"),
        };

        var result = cycleDetector.GetCyclePaths(tables);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void GetCyclePaths_GivenTablesWithCycle_ReturnsCycleTables()
    {
        var cycleDetector = new CycleDetector();

        // create tables with a cycle where the path is a -> b -> c -> a
        var tables = new[]
        {
            CreateTable("a", "b"),
            CreateTable("b", "c"),
            CreateTable("c", "a"),
        };

        var result = cycleDetector.GetCyclePaths(tables);

        var cycleTableNames = GetLocalNames(result);
        var expectedCycle = new[] { "a", "b", "c" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(cycleTableNames, Has.Exactly(3).Items);
            Assert.That(cycleTableNames, Is.EquivalentTo(expectedCycle));
        }
    }

    [Test]
    public static void GetCyclePaths_GivenCycleWithBranchExploredFirst_ReturnsOnlyCycleTables()
    {
        var cycleDetector = new CycleDetector();

        // the cycle is a -> b -> a, while b -> c -> d is a branch leading away from the cycle
        var tables = new[]
        {
            CreateTable("a", "b"),
            CreateTable("b", "c", "a"),
            CreateTable("c", "d"),
            CreateTable("d"),
        };

        var result = cycleDetector.GetCyclePaths(tables);

        var cycleTableNames = GetLocalNames(result);
        var expectedCycle = new[] { "a", "b" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(cycleTableNames, Is.EquivalentTo(expectedCycle));
        }
    }

    [Test]
    public static void GetCyclePaths_GivenCycleTableWithDeadEndRelationship_ReturnsOnlyCycleTables()
    {
        var cycleDetector = new CycleDetector();

        // the cycle is r -> b -> c -> r, while b -> x leaves the cycle and goes nowhere
        var tables = new[]
        {
            CreateTable("r", "b"),
            CreateTable("b", "x", "c"),
            CreateTable("c", "r"),
            CreateTable("x"),
        };

        var result = cycleDetector.GetCyclePaths(tables);

        var cycleTableNames = GetLocalNames(result);
        var expectedCycle = new[] { "r", "b", "c" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(cycleTableNames, Is.EquivalentTo(expectedCycle));
        }
    }

    [Test]
    public static void GetCyclePaths_GivenMultipleDisjointCycles_ReturnsEachCycleSeparately()
    {
        var cycleDetector = new CycleDetector();

        // two independent cycles, a -> b -> a and c -> d -> c, plus an unrelated table
        var tables = new[]
        {
            CreateTable("a", "b"),
            CreateTable("b", "a"),
            CreateTable("c", "d"),
            CreateTable("d", "c"),
            CreateTable("e"),
        };

        var result = cycleDetector.GetCyclePaths(tables);

        var cycles = result.Select(c => c.Select(static t => t.LocalName).ToList()).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Exactly(2).Items);
            Assert.That(cycles, Has.One.EquivalentTo(new[] { "a", "b" }));
            Assert.That(cycles, Has.One.EquivalentTo(new[] { "c", "d" }));
        }
    }

    private static IReadOnlyCollection<string> GetLocalNames(IEnumerable<IReadOnlyCollection<Identifier>> cycles)
    {
        return cycles.SelectMany(static c => c.Select(static t => t.LocalName)).ToList();
    }

    private static IRelationalDatabaseTable CreateTable(string tableName, params string[] parentTableNames)
    {
        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(c => c.KeyType).Returns(DatabaseKeyType.Foreign);
        childKeyMock.Setup(c => c.Columns).Returns([]);

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(p => p.KeyType).Returns(DatabaseKeyType.Primary);
        parentKeyMock.Setup(p => p.Columns).Returns([]);

        var name = Identifier.CreateQualifiedIdentifier(tableName);
        var parentKeys = parentTableNames
            .Select(parentTableName => new DatabaseRelationalKey(
                name,
                childKeyMock.Object,
                Identifier.CreateQualifiedIdentifier(parentTableName),
                parentKeyMock.Object,
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            ))
            .ToList();

        var tableMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableMock.Setup(t => t.Name).Returns(name);
        tableMock.Setup(t => t.ParentKeys).Returns(parentKeys);

        return tableMock.Object;
    }
}
