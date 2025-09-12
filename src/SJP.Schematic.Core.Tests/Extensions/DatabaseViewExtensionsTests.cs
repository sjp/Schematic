using System.Linq;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class DatabaseViewExtensionsTests
{
    private static IDatabaseView GetMockView(Identifier viewName)
    {
        var testColumnType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "INTEGER",
            typeof(int),
            false,
            10,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
        var columns = new[]
        {
            new DatabaseColumn("test_column_1", testColumnType, false, Option<string>.None, Option<IAutoIncrement>.None),
            new DatabaseColumn("test_column_2", testColumnType, false, Option<string>.None, Option<IAutoIncrement>.None),
        };

        var viewMock = new Mock<IDatabaseView>(MockBehavior.Strict);
        viewMock.Setup(v => v.Name).Returns(viewName);
        viewMock.Setup(v => v.Columns).Returns(columns);

        return viewMock.Object;
    }

    [Test]
    public static void GetColumnLookup_GivenNullView_ThrowsArgumentNullException()
    {
        Assert.That(() => DatabaseViewExtensions.GetColumnLookup(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetColumnLookup_ForIdentifierResolvingOverloadGivenNullView_ThrowsArgumentNullException()
    {
        var resolver = new VerbatimIdentifierResolutionStrategy();

        Assert.That(() => DatabaseViewExtensions.GetColumnLookup(null, resolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetColumnLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
    {
        var view = GetMockView("test");

        Assert.That(() => view.GetColumnLookup(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetColumnLookup_GivenViewWithColumns_ReturnsLookupWithExpectedKeys()
    {
        var expectedKeys = new[] { "test_column_1", "test_column_2" };
        var view = GetMockView("test");

        var columnLookup = view.GetColumnLookup();
        var lookupKeys = columnLookup.Keys.Select(c => c.LocalName);

        Assert.That(lookupKeys, Is.EqualTo(expectedKeys));
    }

    [Test]
    public static void GetColumnLookup_ForIdentifierResolvingOverloadGivenViewWithColumns_ReturnsLookupWithExpectedKeys()
    {
        var expectedKeys = new[] { "test_column_1", "test_column_2" };
        var view = GetMockView("test");

        var columnLookup = view.GetColumnLookup(new VerbatimIdentifierResolutionStrategy());
        var lookupKeys = columnLookup.Keys.Select(c => c.LocalName);

        Assert.That(lookupKeys, Is.EqualTo(expectedKeys));
    }
}