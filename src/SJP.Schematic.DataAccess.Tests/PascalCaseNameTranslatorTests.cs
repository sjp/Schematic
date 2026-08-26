using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests;

[TestFixture]
internal static class PascalCaseNameTranslatorTests
{
    [Test]
    public static void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        Assert.That(() => nameTranslator.SchemaToNamespace(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SchemaToNamespace_GivenNullSchema_ReturnsNull()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("test");

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.Null);
    }

    [Test]
    public static void SchemaToNamespace_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("first second", "test");
        const string expected = "Firstsecond";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenUnderscoreSeparatedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("first_second", "test");
        const string expected = "FirstSecond";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenCamelCasedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("FirstSecond", "test");
        const string expected = "FirstSecond";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenNullName_ThrowsArgumentNullException()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        Assert.That(() => nameTranslator.TableToClassName(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void TableToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("first second");
        const string expected = "Firstsecond";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenUnderscoreSeparatedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("first_second");
        const string expected = "FirstSecond";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenCamelCasedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("FirstSecond");
        const string expected = "FirstSecond";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenNullName_ThrowsArgumentNullException()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        Assert.That(() => nameTranslator.ViewToClassName(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ViewToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("first second");
        const string expected = "Firstsecond";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenUnderscoreSeparatedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("first_second");
        const string expected = "FirstSecond";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenCamelCasedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("FirstSecond");
        const string expected = "FirstSecond";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ColumnToPropertyName_GivenNullOrWhiteSpaceClassName_ThrowsArgumentException(string className)
    {
        const string columnName = "test";
        var nameTranslator = new PascalCaseNameTranslator();
        Assert.That(() => nameTranslator.ColumnToPropertyName(className, columnName), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ColumnToPropertyName_GivenNullOrWhiteSpaceColumnName_ThrowsArgumentException(string columnName)
    {
        const string className = "test";
        var nameTranslator = new PascalCaseNameTranslator();
        Assert.That(() => nameTranslator.ColumnToPropertyName(className, columnName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void ColumnToPropertyName_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "first second";
        const string expected = "Firstsecond";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenUnderscoreSeparatedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "first_second";
        const string expected = "FirstSecond";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenCamelCasedSchemaName_ReturnsPascalCasedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "FirstSecond";
        const string expected = "FirstSecond";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenTransformedNameMatchingClassName_ReturnsUnderscoreAppendedColumnName()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "FirstSecond";
        const string testName = "FirstSecond";
        const string expected = "FirstSecond_";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenDigitLeadingSchemaName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("1entries", "test");
        const string expected = "_1entries";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenSymbolLeadingSchemaName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("$entries", "test");
        const string expected = "_entries";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenDigitLeadingLocalName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("1entries");
        const string expected = "_1entries";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenSymbolLeadingLocalName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("$entries");
        const string expected = "_entries";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenDigitLeadingLocalName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("1entries");
        const string expected = "_1entries";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenSymbolLeadingLocalName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("$entries");
        const string expected = "_entries";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenDigitLeadingColumnName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "1entries";
        const string expected = "_1entries";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenSymbolLeadingColumnName_ReturnsUnderscorePrefixedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "$entries";
        const string expected = "_entries";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void SchemaToNamespace_GivenKeywordSchemaName_ReturnsEscapedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("class", "test");
        const string expected = "Class";

        var result = nameTranslator.SchemaToNamespace(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void TableToClassName_GivenKeywordLocalName_ReturnsEscapedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("event");
        const string expected = "Event";

        var result = nameTranslator.TableToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ViewToClassName_GivenKeywordLocalName_ReturnsEscapedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();
        var testName = new Identifier("params");
        const string expected = "Params";

        var result = nameTranslator.ViewToClassName(testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenKeywordColumnName_ReturnsEscapedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "class";
        const string expected = "Class";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public static void ColumnToPropertyName_GivenColumnNameCasingToAKeyword_ReturnsEscapedText()
    {
        var nameTranslator = new PascalCaseNameTranslator();

        const string className = "test";
        const string testName = "Class";
        const string expected = "Class";

        var result = nameTranslator.ColumnToPropertyName(className, testName);
        Assert.That(result, Is.EqualTo(expected));
    }
}
