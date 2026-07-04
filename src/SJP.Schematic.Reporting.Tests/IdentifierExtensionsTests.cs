using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Tests;

[TestFixture]
internal static partial class IdentifierExtensionsTests
{
    [Test]
    public static void ToVisibleName_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => IdentifierExtensions.ToVisibleName(null!), Throws.ArgumentNullException);
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
        Assert.That(() => IdentifierExtensions.ToSafeKey(null!), Throws.ArgumentNullException);
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

    private const int HashSuffixLength = 9; // separator + 8 hex characters

    [GeneratedRegex(@"^.+-[0-9a-f]{8}$")]
    private static partial Regex SafeKeyPattern();
}
