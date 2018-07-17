using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class CamelCaseNameProviderTests
    {
        [Test]
        public static void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
        {
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.SchemaToNamespace(null));
        }

        [Test]
        public static void SchemaToNamespace_GivenNullSchema_ReturnsNull()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("asd");

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.IsNull(result);
        }

        [Test]
        public static void SchemaToNamespace_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("first second", "asd");
            const string expected = "firstsecond";

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void SchemaToNamespace_GivenUnderscoreSeparatedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("first_second", "asd");
            const string expected = "firstSecond";

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void SchemaToNamespace_GivenPascalCasedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("FirstSecond", "asd");
            const string expected = "firstSecond";

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void TableToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.TableToClassName(null));
        }

        [Test]
        public static void TableToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameProvider.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void TableToClassName_GivenUnderscoreSeparatedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("first_second");
            const string expected = "firstSecond";

            var result = nameProvider.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void TableToClassName_GivenPascalCasedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("FirstSecond");
            const string expected = "firstSecond";

            var result = nameProvider.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ViewToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ViewToClassName(null));
        }

        [Test]
        public static void ViewToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameProvider.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ViewToClassName_GivenUnderscoreSeparatedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("first_second");
            const string expected = "firstSecond";

            var result = nameProvider.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ViewToClassName_GivenPascalCasedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();
            var testName = new Identifier("FirstSecond");
            const string expected = "firstSecond";

            var result = nameProvider.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenNullClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(null, columnName));
        }

        [Test]
        public static void ColumnToPropertyName_GivenEmptyClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(string.Empty, columnName));
        }

        [Test]
        public static void ColumnToPropertyName_GivenWhiteSpaceClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName("    ", columnName));
        }

        [Test]
        public static void ColumnToPropertyName_GivenNullColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(className, null));
        }

        [Test]
        public static void ColumnToPropertyName_GivenEmptyColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(className, string.Empty));
        }

        [Test]
        public static void ColumnToPropertyName_GivenWhiteSpaceColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameProvider = new CamelCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(className, "    "));
        }

        [Test]
        public static void ColumnToPropertyName_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new CamelCaseNameProvider();

            const string className = "test";
            const string testName = "first second";
            const string expected = "firstsecond";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenUnderscoreSeparatedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();

            const string className = "test";
            const string testName = "first_second";
            const string expected = "firstSecond";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenPascalCasedSchemaName_ReturnsCamelCasedText()
        {
            var nameProvider = new CamelCaseNameProvider();

            const string className = "test";
            const string testName = "FirstSecond";
            const string expected = "firstSecond";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ColumnToPropertyName_GivenTransformedNameMatchingClassName_ReturnsUnderscoreAppendedColumnName()
        {
            var nameProvider = new CamelCaseNameProvider();

            const string className = "firstSecond";
            const string testName = "FirstSecond";
            const string expected = "firstSecond_";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }
    }
}
