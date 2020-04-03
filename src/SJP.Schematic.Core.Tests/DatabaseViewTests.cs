using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseViewTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.That(() => new DatabaseView(null, definition, columns), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
        {
            Identifier viewName = "test_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.That(() => new DatabaseView(viewName, definition, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";

            Assert.That(() => new DatabaseView(viewName, definition, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            Assert.That(view.Name, Is.EqualTo(viewName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            Assert.That(view.Definition, Is.EqualTo(definition));
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";

            Identifier columnName = "star";
            var columnMock = new Mock<IDatabaseColumn>();
            columnMock.SetupGet(c => c.Name).Returns(columnName);
            var columns = new[] { columnMock.Object };

            var view = new DatabaseView(viewName, definition, columns);
            var viewColumnName = view.Columns[0].Name;

            Assert.That(viewColumnName, Is.EqualTo(columnName));
        }

        [Test]
        public static void IsMaterialized_PropertyGet_ReturnsFalse()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            Assert.That(view.IsMaterialized, Is.False);
        }

        [TestCase("", "test_view", "View: test_view")]
        [TestCase("test_schema", "test_view", "View: test_schema.test_view")]
        public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
        {
            var viewName = Identifier.CreateQualifiedIdentifier(schema, localName);
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            var result = view.ToString();

            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
