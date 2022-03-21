using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests;

[TestFixture]
internal static class VerbatimNameTranslatorTests
{
    [Test]
    public static void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => nameTranslator.SchemaToNamespace(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SchemaToNamespace_GivenNullSchema_ReturnsNull()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("test");

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.Null);
    }

    [Test]
    public static void SchemaToNamespace_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("first second", "test");
        const string expected = "firstsecond";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenUnderscoreSeparatedSchemaName_ReturnsUnderscoreSeparatedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("first_second", "test");
        const string expected = "first_second";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenCamelCasedSchemaName_ReturnsCamelCasedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("firstSecond", "test");
        const string expected = "firstSecond";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenNullName_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => nameTranslator.TableToClassName(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void TableToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("first second");
        const string expected = "firstsecond";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenUnderscoreSeparatedSchemaName_ReturnsUnderscoreSeparatedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("first_second");
        const string expected = "first_second";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenPascalCasedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("FirstSecond");
        const string expected = "FirstSecond";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenNullName_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => nameTranslator.ViewToClassName(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ViewToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("first second");
        const string expected = "firstsecond";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenUnderscoreSeparatedSchemaName_ReturnsUnderscoreSeparatedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("first_second");
        const string expected = "first_second";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenCamelCasedSchemaName_ReturnsCamelCasedText()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var testName = new Identifier("firstSecond");
        const string expected = "firstSecond";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ColumnToPropertyName_GivenNullOrWhiteSpaceClassName_ThrowsArgumentNullException(string className)
    {
        const string columnName = "test";
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => nameTranslator.ColumnToPropertyName(className, columnName), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ColumnToPropertyName_GivenNullOrWhiteSpaceColumnName_ThrowsArgumentNullException(string columnName)
    {
        const string className = "test";
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => nameTranslator.ColumnToPropertyName(className, columnName), Throws.ArgumentNullException);
    }

    [Test]
    public static void ColumnToPropertyName_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new VerbatimNameTranslator();

        const string className = "test";
        const string testName = "first second";
        const string expected = "firstsecond";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenUnderscoreSeparatedSchemaName_ReturnsUnderscoreSeparatedText()
    {
        var nameTranslator = new VerbatimNameTranslator();

        const string className = "test";
        const string testName = "first_second";
        const string expected = "first_second";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenPascalCasedColumnName_ReturnsPascalCasedText()
    {
        var nameTranslator = new VerbatimNameTranslator();

        const string className = "test";
        const string testName = "FirstSecond";
        const string expected = "FirstSecond";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenTransformedNameMatchingClassName_ReturnsUnderscoreAppendedColumnName()
    {
        var nameTranslator = new VerbatimNameTranslator();

        const string className = "FirstSecond";
        const string testName = "FirstSecond";
        const string expected = "FirstSecond_";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }
}