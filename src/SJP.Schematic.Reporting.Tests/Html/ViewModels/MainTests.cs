using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class MainTests
{
    private static readonly Identifier TableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");
    private static readonly Identifier SequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_sequence");
    private static readonly Identifier SynonymName = Identifier.CreateQualifiedIdentifier("test_schema", "test_synonym");
    private static readonly Identifier RoutineName = Identifier.CreateQualifiedIdentifier("test_schema", "test_routine");
    private static readonly Identifier TypeName = Identifier.CreateQualifiedIdentifier("test_schema", "test_type");

    [Test]
    public static void Ctor_GivenNullSchemas_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main("test_database", "1.0", 1, 1, 1, null!, 1, 1, 1, 1, 1, 1),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("schemas")
        );
    }

    [Test]
    public static void SchemaCtor_GivenNullSchemaName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.Schema(null!, Option<string>.None, true, false, 1, 1, 1, 1, 1, 1),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("schemaName")
        );
    }

    [Test]
    public static void TableCtor_GivenNullTableName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.Table(null!, 0, 0, 1, TableKind.Regular, Option<long>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void TableCtor_GivenInvalidTableKind_ThrowsArgumentException()
    {
        Assert.That(
            () => new Main.Table(TableName, 0, 0, 1, (TableKind)55, Option<long>.None),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("kind")
        );
    }

    [Test]
    public static void ViewCtor_GivenNullViewName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.View(null!, 1, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName")
        );
    }

    [Test]
    public static void SequenceCtor_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.Sequence(null!, "bigint", Option<Uri>.None, 1, 1, Option<decimal>.None, Option<decimal>.None, SequenceCacheMode.None, Option<int>.None, false, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }

    [Test]
    public static void SequenceCtor_GivenInvalidSequenceCacheMode_ThrowsArgumentException()
    {
        Assert.That(
            () => new Main.Sequence(SequenceName, "bigint", Option<Uri>.None, 1, 1, Option<decimal>.None, Option<decimal>.None, (SequenceCacheMode)55, Option<int>.None, false, false),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("cacheMode")
        );
    }

    [Test]
    public static void SynonymCtor_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.Synonym(null!, TableName, Option<Uri>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymName")
        );
    }

    [Test]
    public static void SynonymCtor_GivenNullTarget_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.Synonym(SynonymName, null!, Option<Uri>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("target")
        );
    }

    [Test]
    public static void RoutineCtor_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.Routine(null!, RoutineType.Function),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineName")
        );
    }

    [Test]
    public static void RoutineCtor_GivenInvalidRoutineType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Main.Routine(RoutineName, (RoutineType)55),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineType")
        );
    }

    [Test]
    public static void UserDefinedTypeCtor_GivenNullTypeName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Main.UserDefinedType(null!, UserDefinedTypeKind.Domain, Option<IDbType>.None, Option<Uri>.None, true, 0, 0),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeName")
        );
    }

    [Test]
    public static void UserDefinedTypeCtor_GivenInvalidUserDefinedTypeKind_ThrowsArgumentException()
    {
        Assert.That(
            () => new Main.UserDefinedType(TypeName, (UserDefinedTypeKind)55, Option<IDbType>.None, Option<Uri>.None, true, 0, 0),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("kind")
        );
    }
}
