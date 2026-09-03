using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

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

        Assert.That(() => new DatabaseMaterializedView(viewName, null!, columns), Throws.ArgumentNullException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Identifier viewName = "test_mat_view";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        Assert.That(() => new DatabaseMaterializedView(viewName, definition, columns), Throws.ArgumentException);
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
        var columnMock = new Mock<IDatabaseColumn>(MockBehavior.Strict);
        columnMock.Setup(c => c.Name).Returns(columnName);
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

    [Test]
    public static void RefreshMetadata_WhenNotProvided_IsUnreported()
    {
        Identifier viewName = "test_mat_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseMaterializedView(viewName, definition, columns);

        Assert.Multiple(() =>
        {
            Assert.That(view.RefreshMode, Is.EqualTo(MaterializedViewRefreshMode.Unknown));
            Assert.That(view.RefreshMethod, OptionIs.None);
            Assert.That(view.IsPopulated, Is.False);
            Assert.That(view.Triggers, Is.Empty);
            Assert.That(view.Indexes, Is.Empty);
        });
    }

    [Test]
    public static void RefreshMetadata_PropertyGet_EqualsCtorArg()
    {
        Identifier viewName = "test_mat_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var indexes = new[] { Mock.Of<IDatabaseIndex>() };

        var view = new DatabaseMaterializedView(
            viewName,
            definition,
            columns,
            [],
            indexes,
            MaterializedViewRefreshMode.OnCommit,
            Option<string>.Some("FAST"),
            true
        );

        Assert.Multiple(() =>
        {
            Assert.That(view.RefreshMode, Is.EqualTo(MaterializedViewRefreshMode.OnCommit));
            Assert.That(view.RefreshMethod.UnwrapSome(), Is.EqualTo("FAST"));
            Assert.That(view.IsPopulated, Is.True);
            Assert.That(view.Indexes, Is.EqualTo(indexes));
        });
    }

    [Test]
    public static void Ctor_GivenInvalidRefreshMode_ThrowsArgumentException()
    {
        Identifier viewName = "test_mat_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        const MaterializedViewRefreshMode refreshMode = (MaterializedViewRefreshMode)55;

        Assert.That(
            () => new DatabaseMaterializedView(viewName, definition, columns, [], [], refreshMode, Option<string>.None, false),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void ViewProperties_WhenMaterialized_AreNotUpdatableAndUnchecked()
    {
        Identifier viewName = "test_mat_view";
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseMaterializedView(viewName, definition, columns);

        Assert.Multiple(() =>
        {
            Assert.That(view.CheckOption, Is.EqualTo(ViewCheckOption.None));
            Assert.That(view.IsUpdatable, Is.False);
        });
    }

    [TestCase("", "test_view", "Materialized View: test_view")]
    [TestCase("test_schema", "test_view", "Materialized View: test_schema.test_view")]
    public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
    {
        var viewName = Identifier.CreateQualifiedIdentifier(schema, localName);
        const string definition = "select * from test";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var view = new DatabaseMaterializedView(viewName, definition, columns);

        var result = view.ToString();

        Assert.That(result, Is.EqualTo(expectedOutput));
    }
}