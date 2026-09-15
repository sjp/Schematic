using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

internal static class TablePartitioningTests
{
    [Test]
    public static void Ctor_GivenNullStrategy_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TablePartitioning(null, [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("strategy")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceStrategy_ThrowsArgumentException(string strategy)
    {
        Assert.That(
            () => new TablePartitioning(strategy, [], []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("strategy")
        );
    }

    [Test]
    public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TablePartitioning("RANGE", null, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void Ctor_GivenColumnsWithNullValue_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TablePartitioning("RANGE", [null], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void Ctor_GivenNullPartitions_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TablePartitioning("RANGE", [], null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("partitions")
        );
    }

    [Test]
    public static void Ctor_GivenPartitionsWithNullValue_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new TablePartitioning("RANGE", [], [null]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("partitions")
        );
    }

    [Test]
    public static void Properties_WhenConstructed_RetainGivenValues()
    {
        var column = Mock.Of<IDatabaseColumn>();

        var partitioning = new TablePartitioning("LIST", [column], ["p_first", "p_second"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(partitioning.Strategy, Is.EqualTo("LIST"));
            Assert.That(partitioning.Columns, Is.EqualTo(new[] { column }));
            Assert.That(partitioning.Partitions, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public static void Collections_WhenSourceCollectionsMutatedAfterConstruction_RemainUnchanged()
    {
        var columns = new List<IDatabaseColumn>();
        var partitions = new List<Identifier>();

        var partitioning = new TablePartitioning("HASH", columns, partitions);

        columns.Add(Mock.Of<IDatabaseColumn>());
        partitions.Add("p_first");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(partitioning.Columns, Is.Empty);
            Assert.That(partitioning.Partitions, Is.Empty);
        }
    }

    [Test]
    public static void ToString_WhenInvoked_ReturnsExpectedString()
    {
        var partitioning = new TablePartitioning("RANGE", [], []);

        Assert.That(partitioning.ToString(), Is.EqualTo("Partitioning: RANGE"));
    }
}
