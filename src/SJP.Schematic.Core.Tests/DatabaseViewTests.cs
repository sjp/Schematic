using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

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

    [Test]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Identifier viewName = "test_view";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        Assert.That(() => new DatabaseView(viewName, null!, columns), Throws.ArgumentNullException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Identifier viewName = "test_view";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        Assert.That(() => new DatabaseView(viewName, definition, columns), Throws.ArgumentException);
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
        var columnMock = new Mock<IDatabaseColumn>(MockBehavior.Strict);
        columnMock.Setup(c => c.Name).Returns(columnName);
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

    [Test]
    public static void Triggers_WhenNotProvided_IsEmpty()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseView(viewName, definition, columns);

        Assert.That(view.Triggers, Is.Empty);
    }

    [Test]
    public static void Indexes_WhenNotProvided_IsEmpty()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseView(viewName, definition, columns);

        Assert.That(view.Indexes, Is.Empty);
    }

    [Test]
    public static void CheckOption_WhenNotProvided_IsNone()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseView(viewName, definition, columns);

        Assert.That(view.CheckOption, Is.EqualTo(ViewCheckOption.None));
    }

    [Test]
    public static void IsUpdatable_WhenNotProvided_ReturnsFalse()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseView(viewName, definition, columns);

        Assert.That(view.IsUpdatable, Is.False);
    }

    [Test]
    public static void Ctor_GivenNullTriggers_ThrowsArgumentNullException()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        Assert.That(() => new DatabaseView(viewName, definition, columns, null!, [], ViewCheckOption.None, false), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIndexes_ThrowsArgumentNullException()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        Assert.That(() => new DatabaseView(viewName, definition, columns, [], null!, ViewCheckOption.None, false), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidCheckOption_ThrowsArgumentException()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        const ViewCheckOption checkOption = (ViewCheckOption)55;

        Assert.That(() => new DatabaseView(viewName, definition, columns, [], [], checkOption, false), Throws.ArgumentException);
    }

    [Test]
    public static void Triggers_PropertyGet_EqualsCtorArg()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var triggers = new[] { Mock.Of<IDatabaseTrigger>() };

        var view = new DatabaseView(viewName, definition, columns, triggers, [], ViewCheckOption.None, false);

        Assert.That(view.Triggers, Is.EqualTo(triggers));
    }

    [Test]
    public static void Indexes_PropertyGet_EqualsCtorArg()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var indexes = new[] { Mock.Of<IDatabaseIndex>() };

        var view = new DatabaseView(viewName, definition, columns, [], indexes, ViewCheckOption.None, false);

        Assert.That(view.Indexes, Is.EqualTo(indexes));
    }

    [Test]
    public static void CheckOption_PropertyGet_EqualsCtorArg()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseView(viewName, definition, columns, [], [], ViewCheckOption.Cascaded, true);

        Assert.Multiple(() =>
        {
            Assert.That(view.CheckOption, Is.EqualTo(ViewCheckOption.Cascaded));
            Assert.That(view.IsUpdatable, Is.True);
        });
    }

    [Test]
    public static void Columns_WhenSourceCollectionMutatedAfterConstruction_RemainsUnchanged()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new List<IDatabaseColumn> { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseView(viewName, definition, columns);

        columns.Add(Mock.Of<IDatabaseColumn>());

        Assert.That(view.Columns, Has.Count.EqualTo(1));
    }

    [Test]
    public static void Triggers_WhenSourceCollectionMutatedAfterConstruction_RemainsUnchanged()
    {
        Identifier viewName = "test_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var triggers = new List<IDatabaseTrigger> { Mock.Of<IDatabaseTrigger>() };

        var view = new DatabaseView(viewName, definition, columns, triggers, [], ViewCheckOption.None, false);

        triggers.Add(Mock.Of<IDatabaseTrigger>());

        Assert.That(view.Triggers, Has.Count.EqualTo(1));
    }
}