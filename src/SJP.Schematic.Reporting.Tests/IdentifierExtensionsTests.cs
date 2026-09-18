using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Tests;

internal static partial class IdentifierExtensionsTests
{
    [Test]
    public static void ToVisibleName_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => IdentifierExtensions.ToVisibleName(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifier"));
    }

    [Test]
    public static void ToVisibleName_GivenIdentifierWithoutSchema_ReturnsLocalNameOnly()
    {
        Identifier identifier = "test_table";
        var visibleName = identifier.ToVisibleName();

        Assert.That(visibleName, Is.EqualTo("test_table"));
    }

    [Test]
    public static void ToVisibleName_GivenIdentifierWithSchema_ReturnsSchemaQualifiedName()
    {
        var identifier = new Identifier("test_schema", "test_table");
        var visibleName = identifier.ToVisibleName();

        Assert.That(visibleName, Is.EqualTo("test_schema.test_table"));
    }

    [Test]
    public static void ToSafeKey_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => IdentifierExtensions.ToSafeKey(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifier"));
    }

    [Test]
    public static void ToSafeKey_GivenValidIdentifier_MatchesSlugHyphenHashFormat()
    {
        Identifier identifier = "test_table";
        var safeKey = identifier.ToSafeKey();

        Assert.That(safeKey, Does.Match(SafeKeyPattern()));
    }

    [Test]
    public static void ToSafeKey_GivenSameIdentifierTwice_ReturnsSameKey()
    {
        Identifier first = "test_table";
        Identifier second = "test_table";

        Assert.That(first.ToSafeKey(), Is.EqualTo(second.ToSafeKey()));
    }

    [Test]
    public static void ToSafeKey_GivenIdentifiersDifferingOnlyBySchema_ReturnsDifferentKeys()
    {
        var first = new Identifier("schema_one", "test_table");
        var second = new Identifier("schema_two", "test_table");

        Assert.That(first.ToSafeKey(), Is.Not.EqualTo(second.ToSafeKey()));
    }

    [Test]
    public static void ToSafeKey_GivenIdentifiersDifferingOnlyByDatabase_ReturnsDifferentKeys()
    {
        var first = new Identifier("database_one", "schema", "test_table");
        var second = new Identifier("database_two", "schema", "test_table");

        Assert.That(first.ToSafeKey(), Is.Not.EqualTo(second.ToSafeKey()));
    }

    [Test]
    public static void ToSafeKey_GivenNameWithDiacritics_StripsDiacriticsInSlug()
    {
        Identifier identifier = "Café";
        var safeKey = identifier.ToSafeKey();

        Assert.That(safeKey, Does.StartWith("cafe-"));
    }

    [Test]
    public static void ToSafeKey_GivenNameThatReducesToEmptySlug_FallsBackToPlaceholder()
    {
        Identifier identifier = "+++";
        var safeKey = identifier.ToSafeKey();

        Assert.That(safeKey, Does.StartWith("unnamed-"));
    }

    [Test]
    public static void ToSafeKey_GivenNonAsciiOnlyName_FallsBackToPlaceholder()
    {
        Identifier identifier = "日本語";
        var safeKey = identifier.ToSafeKey();

        Assert.That(safeKey, Does.StartWith("unnamed-"));
    }

    [Test]
    public static void ToSafeKey_GivenVeryLongLocalName_TruncatesSlugPortion()
    {
        var longName = string.Concat(Enumerable.Repeat("a", 100));
        Identifier identifier = longName;

        var safeKey = identifier.ToSafeKey();
        var slugPortion = safeKey[..^HashSuffixLength];

        Assert.That(slugPortion.Length, Is.LessThanOrEqualTo(45));
    }

    [Test]
    public static void ToSafeKey_GivenNamesWithSpacesAndUnderscores_CollapsesToSameSlug()
    {
        // The hash suffix is derived from the raw (un-slugified) name, so it differs between the
        // two -- only the human-readable slug portion is expected to collapse to the same value.
        Identifier withSpaces = "test table name";
        Identifier withUnderscores = "test_table_name";

        var spacesSlug = withSpaces.ToSafeKey()[..^HashSuffixLength];
        var underscoresSlug = withUnderscores.ToSafeKey()[..^HashSuffixLength];

        Assert.That(spacesSlug, Is.EqualTo(underscoresSlug));
    }

    [TestCaseSource(nameof(SafeKeyGoldenCases))]
    public static void ToSafeKey_GivenIdentifier_ReturnsStableKey(Identifier identifier, string expectedKey)
    {
        // Keys are used as file names and routes in generated reports, so they must never change.
        Assert.That(identifier.ToSafeKey(), Is.EqualTo(expectedKey));
    }

    private static IEnumerable<TestCaseData> SafeKeyGoldenCases()
    {
        static TestCaseData Case(string name, Identifier identifier, string expectedKey) =>
            new TestCaseData(identifier, expectedKey).SetArgDisplayNames(name);

        yield return Case("local", new Identifier("test_table"), "test-table-f240ad48");
        yield return Case("schema", new Identifier("dbo", "test_table"), "test-table-b1cefadc");
        yield return Case("database", new Identifier("sakila", "dbo", "test_table"), "test-table-f2c9d7b0");
        yield return Case("server", new Identifier("localhost", "sakila", "dbo", "test_table"), "test-table-60833428");
        yield return Case("mixed case", new Identifier("Public", "Film_Actor"), "film-actor-ede86731");
        yield return Case("diacritics", new Identifier("Café Ångström"), "cafe-angstrom-66d608ad");
        yield return Case("sharp s and ligature", new Identifier("straße ﬁle"), "strae-le-7913d40a");
        yield return Case("cjk", new Identifier("日本語"), "unnamed-01ba3fe7");
        yield return Case("cjk mixed", new Identifier("表 table 1"), "table-1-22d06adc");
        yield return Case("all symbols", new Identifier("+++"), "unnamed-4563cb61");
        yield return Case("hyphens only", new Identifier("---"), "----52598ecf");
        yield return Case("underscore and period", new Identifier("_order.line_item_"), "-order-line-item--f3788695");
        yield return Case("mixed whitespace", new Identifier("  order \t\t line\r\nitem  "), "order-line-item-4172cd2b");
        yield return Case("file name characters", new Identifier("a/b\\c:d*e?f\"g<h>i|j"), "abcdefghij-f28c7f4a");
        yield return Case("long", new Identifier(new string('a', 100)), "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa-136ee6ab");
        yield return Case("long with space at limit", new Identifier(new string('b', 44) + " cdef"), "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb-64f8f105");
        yield return Case("long with spaces", new Identifier("this is a very long table name that goes well past the slug length limit"), "this-is-a-very-long-table-name-that-goes-well-f1238e2d");
        yield return Case("whitespace in qualifier", new Identifier("my schema", "my table"), "my-table-681b54e3");
        yield return Case("very long", new Identifier("srv", "db", "schema", string.Concat(Enumerable.Repeat("long_name ", 300))), "long-name-long-name-long-name-long-name-long--1fb5bba7");
        yield return Case("unpaired surrogate in qualifier", new Identifier("bad\uD800schema", "name"), "name-f918ad6f");
    }

    [Test]
    public static void ToSafeKey_GivenManyDistinctIdentifiersBetweenCalls_ReturnsSameKey()
    {
        var identifier = new Identifier("dbo", "test_table");
        var expectedKey = identifier.ToSafeKey();

        for (var i = 0; i < 40_000; i++)
            _ = new Identifier("dbo", "other_table_" + i.ToString(CultureInfo.InvariantCulture)).ToSafeKey();

        Assert.Multiple(() =>
        {
            Assert.That(identifier.ToSafeKey(), Is.EqualTo(expectedKey));
            Assert.That(new Identifier("dbo", "test_table").ToSafeKey(), Is.EqualTo(expectedKey));
        });
    }

    private const int HashSuffixLength = 9; // separator + 8 hex characters

    [GeneratedRegex(@"^.+-[0-9a-f]{8}$")]
    private static partial Regex SafeKeyPattern();
}
