using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class ColumnsTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
    private const string TableUrl = "#/tables/test_table";

    [Test]
    public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Columns(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [Test]
    public static void ColumnSummaryCtor_GivenNullParentName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Columns.ColumnSummary(null!, Columns.ParentObjectType.Table, TableUrl, 1, "test_column", "integer", Option<Uri>.None, false, Option<string>.None, false, false, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentName")
        );
    }

    [Test]
    public static void ColumnSummaryCtor_GivenNullColumnName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Columns.ColumnSummary(TableName, Columns.ParentObjectType.Table, TableUrl, 1, null!, "integer", Option<Uri>.None, false, Option<string>.None, false, false, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ColumnSummaryCtor_GivenEmptyOrWhiteSpaceColumnName_ThrowsArgumentException(string columnName)
    {
        Assert.That(
            () => new Columns.ColumnSummary(TableName, Columns.ParentObjectType.Table, TableUrl, 1, columnName, "integer", Option<Uri>.None, false, Option<string>.None, false, false, false),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    [Test]
    public static void ColumnSummaryCtor_GivenInvalidParentObjectType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Columns.ColumnSummary(TableName, (Columns.ParentObjectType)55, TableUrl, 1, "test_column", "integer", Option<Uri>.None, false, Option<string>.None, false, false, false),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentType")
        );
    }

    [Test]
    public static void ColumnSummaryCtor_GivenNoParentObjectType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Columns.ColumnSummary(TableName, Columns.ParentObjectType.None, TableUrl, 1, "test_column", "integer", Option<Uri>.None, false, Option<string>.None, false, false, false),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentType")
        );
    }

    [Test]
    public static void ColumnSummaryCtor_GivenNullParentUrl_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Columns.ColumnSummary(TableName, Columns.ParentObjectType.Table, null!, 1, "test_column", "integer", Option<Uri>.None, false, Option<string>.None, false, false, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentUrl")
        );
    }
}
