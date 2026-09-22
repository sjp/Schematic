using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class IndexesTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

    [Test]
    public static void Ctor_GivenNullIndexes_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Indexes(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("indexes")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenNullTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", null!, false, ["test_column"], [IndexColumnOrder.Ascending], [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenNullColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, null!, [IndexColumnOrder.Ascending], [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnNames")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenEmptyColumnNames_ThrowsArgumentException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, [], [IndexColumnOrder.Ascending], [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnNames")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenNullColumnSorts_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, ["test_column"], null!, [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnSorts")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenEmptyColumnSorts_ThrowsArgumentException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, ["test_column"], [], [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnSorts")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenInvalidIndexColumnOrder_ThrowsArgumentException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, ["test_column"], [(IndexColumnOrder)55], [], IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnSorts")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenNullIncludedColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, ["test_column"], [IndexColumnOrder.Ascending], null!, IndexType.BTree, Option<string>.None, true, true, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("includedColumnNames")
        );
    }

    [Test]
    public static void IndexRowCtor_GivenInvalidIndexType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Indexes.IndexRow("test_index", TableName, false, ["test_column"], [IndexColumnOrder.Ascending], [], (IndexType)55, Option<string>.None, true, true, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("indexType")
        );
    }
}
