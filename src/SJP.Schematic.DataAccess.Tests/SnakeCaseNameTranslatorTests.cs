using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class SnakeCaseNameTranslatorTests
    {
        [Test]
        public static void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.SchemaToNamespace(null));
        }

        [Test]
        public static void SchemaToNamespace_GivenNullSchema_ReturnsNull()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("asd");

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.IsNull(result);
        }

        [Test]
        public static void SchemaToNamespace_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first second", "asd");
            const string expected = "firstsecond";

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void SchemaToNamespace_GivenUnderscoreSeparatedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first_second", "asd");
            const string expected = "first_second";

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void SchemaToNamespace_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("FirstSecond", "asd");
            const string expected = "first_second";

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void TableToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.TableToClassName(null));
        }

        [Test]
        public static void TableToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameTranslator.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void TableToClassName_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("firstSecond");
            const string expected = "first_second";

            var result = nameTranslator.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void TableToClassName_GivenPascalCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("FirstSecond");
            const string expected = "first_second";

            var result = nameTranslator.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ViewToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ViewToClassName(null));
        }

        [Test]
        public static void ViewToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameTranslator.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ViewToClassName_GivenCamelCaseSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first_second");
            const string expected = "first_second";

            var result = nameTranslator.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ViewToClassName_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("FirstSecond");
            const string expected = "first_second";

            var result = nameTranslator.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenNullClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ColumnToPropertyName(null, columnName));
        }

        [Test]
        public static void ColumnToPropertyName_GivenEmptyClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ColumnToPropertyName(string.Empty, columnName));
        }

        [Test]
        public static void ColumnToPropertyName_GivenWhiteSpaceClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ColumnToPropertyName("    ", columnName));
        }

        [Test]
        public static void ColumnToPropertyName_GivenNullColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ColumnToPropertyName(className, null));
        }

        [Test]
        public static void ColumnToPropertyName_GivenEmptyColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ColumnToPropertyName(className, string.Empty));
        }

        [Test]
        public static void ColumnToPropertyName_GivenWhiteSpaceColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.Throws<ArgumentNullException>(() => nameTranslator.ColumnToPropertyName(className, "    "));
        }

        [Test]
        public static void ColumnToPropertyName_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "test";
            const string testName = "first second";
            const string expected = "firstsecond";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenSnakeCasedClassName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "test";
            const string testName = "first_second";
            const string expected = "first_second";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenPascalCasedColumnName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "test";
            const string testName = "FirstSecond";
            const string expected = "first_second";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenTransformedNameMatchingClassName_ReturnsUnderscoreAppendedColumnName()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "first_second";
            const string testName = "firstSecond";
            const string expected = "first_second_";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }
    }
}
