using System;
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

            Assert.Throws<ArgumentNullException>(() => new DatabaseView(null, definition, columns));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.Throws<ArgumentNullException>(() => new DatabaseView(viewName, null, columns));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.Throws<ArgumentNullException>(() => new DatabaseView(viewName, string.Empty, columns));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.Throws<ArgumentNullException>(() => new DatabaseView(viewName, "    ", columns));
        }

        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";

            Assert.Throws<ArgumentNullException>(() => new DatabaseView(viewName, definition, null));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            Assert.AreEqual(viewName, view.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            Assert.AreEqual(definition, view.Definition);
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

            Assert.AreEqual(columnName, viewColumnName);
        }

        [Test]
        public static void IsMaterialized_PropertyGet_ReturnsFalse()
        {
            Identifier viewName = "test_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseView(viewName, definition, columns);

            Assert.IsFalse(view.IsMaterialized);
        }
    }
}
