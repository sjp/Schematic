using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class EnumerableExtensionsTests
{
    [Test]
    public static void NullOrEmpty_GivenNullCollection_ReturnsTrue()
    {
        IEnumerable<string> input = null;
        Assert.That(input.NullOrEmpty(), Is.True);
    }

    [Test]
    public static void NullOrEmpty_GivenEmptyCollection_ReturnsTrue()
    {
        IEnumerable<string> input = [];
        Assert.That(input.NullOrEmpty(), Is.True);
    }

    [Test]
    public static void NullOrEmpty_GivenNonEmptyCollection_ReturnsFalse()
    {
        IEnumerable<string> input = ["A"];
        Assert.That(input.NullOrEmpty(), Is.False);
    }

    [Test]
    public static void Empty_GivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<string> input = null;
        Assert.That(() => input.Empty(), Throws.ArgumentNullException);
    }

    [Test]
    public static void Empty_GivenEmptyCollection_ReturnsTrue()
    {
        IEnumerable<string> input = [];
        Assert.That(input.Empty(), Is.True);
    }

    [Test]
    public static void Empty_GivenNonEmptyCollection_ReturnsFalse()
    {
        IEnumerable<string> input = ["A"];
        Assert.That(input.Empty(), Is.False);
    }

    [Test]
    public static void NullOrAnyNull_GivenNullCollection_ReturnsTrue()
    {
        IEnumerable<string> input = null;
        Assert.That(input.NullOrAnyNull(), Is.True);
    }

    [Test]
    public static void NullOrAnyNull_GivenEmptyCollection_ReturnsFalse()
    {
        IEnumerable<string> input = [];
        Assert.That(input.NullOrAnyNull(), Is.False);
    }

    [Test]
    public static void NullOrAnyNull_GivenNonEmptyCollectionWithNoNullValues_ReturnsFalse()
    {
        IEnumerable<string> input = ["A"];
        Assert.That(input.NullOrAnyNull(), Is.False);
    }

    [Test]
    public static void NullOrAnyNull_GivenNonEmptyCollectionWithNullValues_ReturnsTrue()
    {
        IEnumerable<string> input = ["A", null, "C"];
        Assert.That(input.NullOrAnyNull(), Is.True);
    }

    [Test]
    public static void AnyNull_GivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<string> input = null;
        Assert.That(() => input.AnyNull(), Throws.ArgumentNullException);
    }

    [Test]
    public static void AnyNull_GivenEmptyCollection_ReturnsFalse()
    {
        IEnumerable<string> input = [];
        Assert.That(input.AnyNull(), Is.False);
    }

    [Test]
    public static void AnyNull_GivenNonEmptyCollectionWithNoNullValues_ReturnsFalse()
    {
        IEnumerable<string> input = ["A"];
        Assert.That(input.AnyNull(), Is.False);
    }

    [Test]
    public static void AnyNull_GivenNonEmptyCollectionWithNullValues_ReturnsTrue()
    {
        IEnumerable<string> input = ["A", null, "C"];
        Assert.That(input.AnyNull(), Is.True);
    }

    [Test]
    public static void GroupAsDictionary_GivenNullCollection_ThrowsArgumentNullException()
    {
        Assert.That(() => EnumerableExtensions.GroupAsDictionary<string, string>(null, x => x), Throws.ArgumentNullException);
    }

    [Test]
    public static void GroupAsDictionary_GivenNullSelector_ThrowsArgumentNullException()
    {
        var source = new[] { "first", "second", "third", "fourth", "fifth" };

        Assert.That(() => source.GroupAsDictionary<string, string>(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GroupAsDictionary_ForComparerOverloadGivenNullCollection_ThrowsArgumentNullException()
    {
        Assert.That(() => EnumerableExtensions.GroupAsDictionary<string, string>(null, x => x, StringComparer.Ordinal), Throws.ArgumentNullException);
    }

    [Test]
    public static void GroupAsDictionary_ForComparerOverloadGivenNullSelector_ThrowsArgumentNullException()
    {
        var source = new[] { "first", "second", "third", "fourth", "fifth" };

        Assert.That(() => source.GroupAsDictionary(null, StringComparer.Ordinal), Throws.ArgumentNullException);
    }

    [Test]
    public static void GroupAsDictionary_GivenValidSelector_ReturnsExpectedResult()
    {
        var source = new[] { "a", "bb", "ccc", "ddd", "eeeee" };
        var grouping = source.GroupAsDictionary(x => x.Length);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(grouping, Has.Exactly(4).Items);

            Assert.That(grouping, Does.ContainKey(1));
            Assert.That(grouping[1], Is.EquivalentTo(new[] { "a" }));

            Assert.That(grouping, Does.ContainKey(2));
            Assert.That(grouping[2], Is.EquivalentTo(new[] { "bb" }));

            Assert.That(grouping, Does.ContainKey(3));
            Assert.That(grouping[3], Is.EquivalentTo(new[] { "ccc", "ddd" }));

            Assert.That(grouping, Does.Not.ContainKey(4));

            Assert.That(grouping, Does.ContainKey(5));
            Assert.That(grouping[5], Is.EquivalentTo(new[] { "eeeee" }));
        }
    }

    [Test]
    public static void GroupAsDictionary_GivenCustomComparer_ReturnsExpectedResult()
    {
        var source = new[] { "a", "bb", "ccc", "CCC", "eeeee" };
        var grouping = source.GroupAsDictionary(x => x, StringComparer.OrdinalIgnoreCase);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(grouping, Has.Exactly(4).Items);

            Assert.That(grouping, Does.ContainKey("a"));
            Assert.That(grouping["a"], Is.EquivalentTo(new[] { "a" }));

            Assert.That(grouping, Does.ContainKey("bb"));
            Assert.That(grouping["bb"], Is.EquivalentTo(new[] { "bb" }));

            Assert.That(grouping, Does.ContainKey("ccc"));
            Assert.That(grouping["ccc"], Is.EquivalentTo(new[] { "ccc", "CCC" }));

            Assert.That(grouping, Does.ContainKey("eeeee"));
            Assert.That(grouping["eeeee"], Is.EquivalentTo(new[] { "eeeee" }));
        }
    }

    [Test]
    public static void GroupAsDictionary_GivenNullComparer_ReturnsSameResultsAsNoParam()
    {
        var source = new[] { "a", "bb", "ccc", "CCC", "eeeee" };
        var grouping = source.GroupAsDictionary(x => x, null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(grouping, Has.Exactly(5).Items);

            Assert.That(grouping, Does.ContainKey("a"));
            Assert.That(grouping["a"], Is.EquivalentTo(new[] { "a" }));

            Assert.That(grouping, Does.ContainKey("bb"));
            Assert.That(grouping["bb"], Is.EquivalentTo(new[] { "bb" }));

            Assert.That(grouping, Does.ContainKey("ccc"));
            Assert.That(grouping["ccc"], Is.EquivalentTo(new[] { "ccc" }));

            Assert.That(grouping, Does.ContainKey("CCC"));
            Assert.That(grouping["CCC"], Is.EquivalentTo(new[] { "CCC" }));

            Assert.That(grouping, Does.ContainKey("eeeee"));
            Assert.That(grouping["eeeee"], Is.EquivalentTo(new[] { "eeeee" }));
        }
    }
}