using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal class SnakeCaseNameProviderTests
    {
        [Test]
        public void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
        {
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.SchemaToNamespace(null));
        }

        [Test]
        public void SchemaToNamespace_GivenNullSchema_ReturnsNull()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("asd");

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.IsNull(result);
        }

        [Test]
        public void SchemaToNamespace_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("first second", "asd");
            const string expected = "firstsecond";

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SchemaToNamespace_GivenUnderscoreSeparatedSchemaName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("first_second", "asd");
            const string expected = "first_second";

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SchemaToNamespace_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("FirstSecond", "asd");
            const string expected = "first_second";

            var result = nameProvider.SchemaToNamespace(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TableToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.TableToClassName(null));
        }

        [Test]
        public void TableToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameProvider.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TableToClassName_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("firstSecond");
            const string expected = "first_second";

            var result = nameProvider.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TableToClassName_GivenPascalCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("FirstSecond");
            const string expected = "first_second";

            var result = nameProvider.TableToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ViewToClassName_GivenNullName_ThrowsArgumentNullException()
        {
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ViewToClassName(null));
        }

        [Test]
        public void ViewToClassName_GivenSpaceSeparatedLocalName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("first second");
            const string expected = "firstsecond";

            var result = nameProvider.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ViewToClassName_GivenCamelCaseSchemaName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("first_second");
            const string expected = "first_second";

            var result = nameProvider.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ViewToClassName_GivenCamelCasedSchemaName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();
            var testName = new Identifier("FirstSecond");
            const string expected = "first_second";

            var result = nameProvider.ViewToClassName(testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ColumnToPropertyName_GivenNullClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(null, columnName));
        }

        [Test]
        public void ColumnToPropertyName_GivenEmptyClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(string.Empty, columnName));
        }

        [Test]
        public void ColumnToPropertyName_GivenWhiteSpaceClassName_ThrowsArgumentNullException()
        {
            const string columnName = "test";
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName("    ", columnName));
        }

        [Test]
        public void ColumnToPropertyName_GivenNullColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(className, null));
        }

        [Test]
        public void ColumnToPropertyName_GivenEmptyColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(className, string.Empty));
        }

        [Test]
        public void ColumnToPropertyName_GivenWhiteSpaceColumnName_ThrowsArgumentNullException()
        {
            const string className = "test";
            var nameProvider = new SnakeCaseNameProvider();
            Assert.Throws<ArgumentNullException>(() => nameProvider.ColumnToPropertyName(className, "    "));
        }

        [Test]
        public void ColumnToPropertyName_GivenSpaceSeparatedSchemaName_ReturnsSpaceRemovedText()
        {
            var nameProvider = new SnakeCaseNameProvider();

            const string className = "test";
            const string testName = "first second";
            const string expected = "firstsecond";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ColumnToPropertyName_GivenSnakeCasedClassName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();

            const string className = "test";
            const string testName = "first_second";
            const string expected = "first_second";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ColumnToPropertyName_GivenPascalCasedColumnName_ReturnsSnakeCasedText()
        {
            var nameProvider = new SnakeCaseNameProvider();

            const string className = "test";
            const string testName = "FirstSecond";
            const string expected = "first_second";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ColumnToPropertyName_GivenTransformedNameMatchingClassName_ReturnsUnderscoreAppendedColumnName()
        {
            var nameProvider = new SnakeCaseNameProvider();

            const string className = "first_second";
            const string testName = "firstSecond";
            const string expected = "first_second_";

            var result = nameProvider.ColumnToPropertyName(className, testName);
            Assert.AreEqual(expected, result);
        }
    }
}
