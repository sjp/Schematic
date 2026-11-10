using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

internal static class TableRelationshipOrdererTests
{
    // creates tables where the foreign key path is a -> b -> c
    private static IReadOnlyList<IRelationalDatabaseTable> CreateTables()
    {
        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        IRelationalDatabaseTable CreateTable(string name, string parentName)
        {
            var tableMock = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
            tableMock.Setup(t => t.Name).Returns(Identifier.CreateQualifiedIdentifier(name));
            tableMock.Setup(t => t.ParentKeys).Returns(parentName == null
                ? []
                :
                [
                    new DatabaseRelationalKey(
                        Identifier.CreateQualifiedIdentifier(name),
                        childKey,
                        Identifier.CreateQualifiedIdentifier(parentName),
                        parentKey,
                        ReferentialAction.NoAction,
                        ReferentialAction.NoAction
                    ),
                ]);

            return tableMock.Object;
        }

        return [CreateTable("a", "b"), CreateTable("b", "c"), CreateTable("c", null)];
    }

    [Test]
    public static void GetDeletionOrder_GivenNullTables_ThrowsArgumentNullException()
    {
        var tableOrder = new TableRelationshipOrderer();

        Assert.That(
            () => tableOrder.GetDeletionOrder(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [Test]
    public static void GetInsertionOrder_GivenNullTables_ThrowsArgumentNullException()
    {
        var tableOrder = new TableRelationshipOrderer();

        Assert.That(
            () => tableOrder.GetInsertionOrder(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables")
        );
    }

    [Test]
    public static void GetDeletionOrder_GivenRelatedTables_ReturnsChildTablesBeforeParentTables()
    {
        var tableOrder = new TableRelationshipOrderer();

        var result = tableOrder.GetDeletionOrder(CreateTables());

        Assert.That(result.Select(t => t.LocalName), Is.EqualTo(new[] { "a", "b", "c" }).AsCollection);
    }

    [Test]
    public static void GetInsertionOrder_GivenRelatedTables_ReturnsParentTablesBeforeChildTables()
    {
        var tableOrder = new TableRelationshipOrderer();

        var result = tableOrder.GetInsertionOrder(CreateTables());

        Assert.That(result.Select(t => t.LocalName), Is.EqualTo(new[] { "c", "b", "a" }).AsCollection);
    }

    [Test]
    public static void GetDeletionOrder_GivenDeferredTables_EnumeratesSourceOnce()
    {
        var tableOrder = new TableRelationshipOrderer();
        var tables = CreateTables();
        var enumerationCount = 0;

        IEnumerable<IRelationalDatabaseTable> DeferredTables()
        {
            enumerationCount++;
            foreach (var table in tables)
                yield return table;
        }

        _ = tableOrder.GetDeletionOrder(DeferredTables());

        Assert.That(enumerationCount, Is.EqualTo(1));
    }
}
