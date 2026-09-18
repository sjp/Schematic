using NUnit.Framework;
using SJP.Schematic.Lint.Naming;

namespace SJP.Schematic.Lint.Tests.Naming;

internal static class NameSimilarityTests
{
    [TestCase("", "", 0)]
    [TestCase("address", "address", 0)]
    [TestCase("address", "adress", 1)]
    [TestCase("address", "", 7)]
    [TestCase("", "address", 7)]
    [TestCase("quantity", "quanitty", 1)]
    [TestCase("abcd", "abdc", 1)]
    [TestCase("kitten", "sitting", 3)]
    [TestCase("description", "descr", 6)]
    [TestCase("address", "addresses", 2)]
    public static void DamerauLevenshteinDistance_GivenTwoNames_ReturnsExpectedDistance(string first, string second, int expected)
    {
        Assert.That(NameSimilarity.DamerauLevenshteinDistance(first, second), Is.EqualTo(expected));
    }

    // A name longer than the rolling rows the distance takes from the stack, to pin that the
    // heap fallback produces the same answer.
    [Test]
    public static void DamerauLevenshteinDistance_GivenNamesLongerThanTheStackBuffer_ReturnsExpectedDistance()
    {
        var first = new string('a', 200);
        var second = new string('a', 199) + "b";

        Assert.That(NameSimilarity.DamerauLevenshteinDistance(first, second), Is.EqualTo(1));
    }

    [TestCase("address", "adress", true)]
    [TestCase("address", "addresss", true)]
    [TestCase("abcd", "abdc", true)]
    [TestCase("address", "address", false)]
    [TestCase("address", "addresses", false)]
    [TestCase("description", "descr", false)]
    public static void AreWithinDistanceOne_GivenTwoNames_ReturnsExpectedResult(string first, string second, bool expected)
    {
        Assert.That(NameSimilarity.AreWithinDistanceOne(first, second), Is.EqualTo(expected));
    }

    [TestCase("order", "orders", true)]
    [TestCase("orders", "order", true)]
    [TestCase("tag", "tags", true)]
    [TestCase("address", "adress", false)]
    [TestCase("order", "order", false)]
    [TestCase("order", "ordera", false)]
    [TestCase("address", "addresses", false)]
    public static void DiffersOnlyByTrailingPluralS_GivenTwoNames_ReturnsExpectedResult(string first, string second, bool expected)
    {
        Assert.That(NameSimilarity.DiffersOnlyByTrailingPluralS(first, second), Is.EqualTo(expected));
    }

    [TestCase("address1", "address2", true)]
    [TestCase("address1", "address", true)]
    [TestCase("line1", "line2", true)]
    [TestCase("1address", "address", true)]
    [TestCase("address", "adress", false)]
    [TestCase("address", "address", true)]
    [TestCase("address1", "adress2", false)]
    public static void DiffersOnlyByDigits_GivenTwoNames_ReturnsExpectedResult(string first, string second, bool expected)
    {
        Assert.That(NameSimilarity.DiffersOnlyByDigits(first, second), Is.EqualTo(expected));
    }
}
