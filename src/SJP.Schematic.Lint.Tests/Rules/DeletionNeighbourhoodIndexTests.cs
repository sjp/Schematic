using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

internal static class DeletionNeighbourhoodIndexTests
{
    [Test]
    public static void Ctor_GivenNullNames_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new DeletionNeighbourhoodIndex(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("names")
        );
    }

    [Test]
    public static void GetDistanceOneMatches_GivenNullName_ThrowsArgumentNullException()
    {
        var index = new DeletionNeighbourhoodIndex(["address"]);
        Assert.That(
            () => index.GetDistanceOneMatches(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name")
        );
    }

    [Test]
    public static void GetDistanceOneMatches_GivenNameInTheIndex_DoesNotReturnTheNameItself()
    {
        var index = new DeletionNeighbourhoodIndex(["address"]);

        Assert.That(index.GetDistanceOneMatches("address"), Is.Empty);
    }

    [TestCase("adress", "address", Description = "an insertion")]
    [TestCase("addresss", "address", Description = "a deletion")]
    [TestCase("addrass", "address", Description = "a substitution")]
    [TestCase("adderss", "address", Description = "a transposition")]
    public static void GetDistanceOneMatches_GivenNameOneEditAway_ReturnsIt(string name, string indexed)
    {
        var index = new DeletionNeighbourhoodIndex([indexed]);

        Assert.That(index.GetDistanceOneMatches(name), Is.EqualTo(new[] { indexed }));
    }

    [Test]
    public static void GetDistanceOneMatches_GivenNameTwoEditsAway_ReturnsNothing()
    {
        var index = new DeletionNeighbourhoodIndex(["address"]);

        Assert.That(index.GetDistanceOneMatches("adres"), Is.Empty);
    }

    // 'ax' and 'ay' share the deletion key 'a' without being one edit from 'az', which is what the
    // verification step after the index lookup exists to reject.
    [Test]
    public static void GetDistanceOneMatches_GivenNamesSharingAKeyButNotADistance_ReturnsOnlyTheVerifiedOnes()
    {
        var index = new DeletionNeighbourhoodIndex(["abcx", "abcy", "zzzz"]);

        Assert.That(index.GetDistanceOneMatches("abcz"), Is.EquivalentTo(new[] { "abcx", "abcy" }));
    }

    // The same answer as comparing every pair, which is what the index is a faster way of doing.
    [Test]
    public static void GetDistanceOneMatches_GivenManyNames_AgreesWithAnAllPairsComparison()
    {
        string[] names =
        [
            "address", "adress", "addresses", "amount", "xamount", "quantity", "quanitty",
            "customerid", "custmerid", "order", "orders", "id", "idx", "a", "",
        ];
        var index = new DeletionNeighbourhoodIndex(names);

        foreach (var name in names)
        {
            var expected = names
                .Where(other => !string.Equals(other, name, StringComparison.Ordinal))
                .Where(other => NameSimilarity.DamerauLevenshteinDistance(name, other) == 1)
                .ToList();

            Assert.That(index.GetDistanceOneMatches(name), Is.EquivalentTo(expected), name);
        }
    }
}
