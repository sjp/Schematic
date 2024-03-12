using System;
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

        var mockChildKey = new Mock<IDatabaseKey>(MockBehavior.Strict);
        mockChildKey.Setup(c => c.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = mockChildKey.Object;

        var mockParentKey = new Mock<IDatabaseKey>(MockBehavior.Strict);
        mockParentKey.Setup(p => p.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = mockParentKey.Object;

        // create tables with no cycle where the path is a -> b -> c
        var tableAMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableAMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier("a"));
        tableAMock.Setup(t => t.ParentKeys).Returns(new[]
        {
            new DatabaseRelationalKey(
                Identifier.CreateQualifiedIdentifier("a"),
                childKey,
                Identifier.CreateQualifiedIdentifier("b"),
                parentKey,
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            )
        });

        var tableBMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableBMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier("b"));
        tableBMock.Setup(t => t.ParentKeys).Returns(new[]
        {
            new DatabaseRelationalKey(
                Identifier.CreateQualifiedIdentifier("b"),
                childKey,
                Identifier.CreateQualifiedIdentifier("c"),
                parentKey,
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            )
        });

        var tableCMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableCMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier("c"));
        tableCMock.Setup(t => t.ParentKeys).Returns([]);

        var tables = new[]
        {
            tableAMock.Object,
            tableBMock.Object,
            tableCMock.Object
        };

        var result = cycleDetector.GetCyclePaths(tables);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void GetCyclePaths_GivenTablesWithCycle_ReturnsEmptyCollection()
    {
        var cycleDetector = new CycleDetector();

        var mockChildKey = new Mock<IDatabaseKey>(MockBehavior.Strict);
        mockChildKey.Setup(c => c.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = mockChildKey.Object;

        var mockParentKey = new Mock<IDatabaseKey>(MockBehavior.Strict);
        mockParentKey.Setup(p => p.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = mockParentKey.Object;

        // create tables with no cycle where the path is a -> b -> c -> a
        var tableAMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableAMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier("a"));
        tableAMock.Setup(t => t.ParentKeys).Returns(new[]
        {
            new DatabaseRelationalKey(
                Identifier.CreateQualifiedIdentifier("a"),
                childKey,
                Identifier.CreateQualifiedIdentifier("b"),
                parentKey,
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            )
        });

        var tableBMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableBMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier("b"));
        tableBMock.Setup(t => t.ParentKeys).Returns(new[]
        {
            new DatabaseRelationalKey(
                Identifier.CreateQualifiedIdentifier("b"),
                childKey,
                Identifier.CreateQualifiedIdentifier("c"),
                parentKey,
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            )
        });

        var tableCMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        tableCMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier("c"));
        tableCMock.Setup(t => t.ParentKeys).Returns(new[]
        {
            new DatabaseRelationalKey(
                Identifier.CreateQualifiedIdentifier("c"),
                childKey,
                Identifier.CreateQualifiedIdentifier("a"),
                parentKey,
                ReferentialAction.NoAction,
                ReferentialAction.NoAction
            )
        });

        var tables = new[]
        {
            tableAMock.Object,
            tableBMock.Object,
            tableCMock.Object
        };

        var result = cycleDetector.GetCyclePaths(tables);

        var cycleTableNames = result.SelectMany(c => c.Select(t => t.LocalName)).ToList();
        var expectedCycle = new[] { "a", "b", "c" };

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Exactly(1).Items);
            Assert.That(cycleTableNames, Has.Exactly(3).Items);
            Assert.That(cycleTableNames, Is.EquivalentTo(expectedCycle));
        });
    }
}