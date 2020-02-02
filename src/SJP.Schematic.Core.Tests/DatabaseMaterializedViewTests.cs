using System;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseMaterializedViewTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.That(() => new DatabaseMaterializedView(null, definition, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_mat_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.That(() => new DatabaseMaterializedView(viewName, null, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_mat_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.That(() => new DatabaseMaterializedView(viewName, string.Empty, columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_mat_view";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            Assert.That(() => new DatabaseMaterializedView(viewName, "    ", columns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Identifier viewName = "test_mat_view";
            const string definition = "select * from test";

            Assert.That(() => new DatabaseMaterializedView(viewName, definition, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_mat_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseMaterializedView(viewName, definition, columns);

            Assert.That(view.Name, Is.EqualTo(viewName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_mat_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseMaterializedView(viewName, definition, columns);

            Assert.That(view.Definition, Is.EqualTo(definition));
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_mat_view";
            const string definition = "select * from test";

            Identifier columnName = "star";
            var columnMock = new Mock<IDatabaseColumn>();
            columnMock.SetupGet(c => c.Name).Returns(columnName);
            var columns = new[] { columnMock.Object };

            var view = new DatabaseMaterializedView(viewName, definition, columns);
            var viewColumnName = view.Columns[0].Name;

            Assert.That(viewColumnName, Is.EqualTo(columnName));
        }

        [Test]
        public static void IsMaterialized_PropertyGet_ReturnsTrue()
        {
            Identifier viewName = "test_mat_view";
            const string definition = "select * from test";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };

            var view = new DatabaseMaterializedView(viewName, definition, columns);

            Assert.That(view.IsMaterialized, Is.True);
        }
    }
}
