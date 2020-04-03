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
            Assert.That(() => nameTranslator.SchemaToNamespace(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void SchemaToNamespace_GivenNullSchema_ReturnsNull()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("test");

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.That(result, Is.Null);
        }

        [Test]
        public static void SchemaToNamespace_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first second", "test");
            const string expected = "firstsecond";

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void SchemaToNamespace_GivenUnderscoreSeparatedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first_second", "test");
            const string expected = "first_second";

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void SchemaToNamespace_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("FirstSecond", "test");
            const string expected = "first_second";

            var result = nameTranslator.SchemaToNamespace(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void TableToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.That(() => nameTranslator.TableToClassName(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void TableToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameTranslator.TableToClassName(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void TableToClassName_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("firstSecond");
            const string expected = "first_second";

            var result = nameTranslator.TableToClassName(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void TableToClassName_GivenPascalCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("FirstSecond");
            const string expected = "first_second";

            var result = nameTranslator.TableToClassName(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void ViewToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.That(() => nameTranslator.ViewToClassName(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void ViewToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameTranslator.ViewToClassName(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void ViewToClassName_GivenCamelCaseSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("first_second");
            const string expected = "first_second";

            var result = nameTranslator.ViewToClassName(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void ViewToClassName_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();
            var testName = new Identifier("FirstSecond");
            const string expected = "first_second";

            var result = nameTranslator.ViewToClassName(testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void ColumnToPropertyName_GivenNullOrWhiteSpaceClassName_ThrowsArgumentNullException(string className)
        {
            const string columnName = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.That(() => nameTranslator.ColumnToPropertyName(className, columnName), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void ColumnToPropertyName_GivenNullOrWhiteSpaceColumnName_ThrowsArgumentNullException(string columnName)
        {
            const string className = "test";
            var nameTranslator = new SnakeCaseNameTranslator();
            Assert.That(() => nameTranslator.ColumnToPropertyName(className, columnName), Throws.ArgumentNullException);
        }

        [Test]
        public static void ColumnToPropertyName_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "test";
            const string testName = "first second";
            const string expected = "firstsecond";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void ColumnToPropertyName_GivenSnakeCasedClassName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "test";
            const string testName = "first_second";
            const string expected = "first_second";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void ColumnToPropertyName_GivenPascalCasedColumnName_ReturnsSnakeCasedText()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "test";
            const string testName = "FirstSecond";
            const string expected = "first_second";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public static void ColumnToPropertyName_GivenTransformedNameMatchingClassName_ReturnsUnderscoreAppendedColumnName()
        {
            var nameTranslator = new SnakeCaseNameTranslator();

            const string className = "first_second";
            const string testName = "firstSecond";
            const string expected = "first_second_";

            var result = nameTranslator.ColumnToPropertyName(className, testName);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
