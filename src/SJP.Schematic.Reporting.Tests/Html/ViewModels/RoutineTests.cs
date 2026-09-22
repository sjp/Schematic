using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class RoutineTests
{
    private static readonly Identifier RoutineName = Identifier.CreateQualifiedIdentifier("test_schema", "test_routine");
    private const string Definition = "create function test_routine...";

    [Test]
    public static void Ctor_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine(null!, Definition, RoutineType.Function, Option<string>.None, [], Option<string>.None, Option<Uri>.None, [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routine")
        );
    }

    [Test]
    public static void Ctor_GivenNullReferencedObjects_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine(RoutineName, Definition, RoutineType.Function, Option<string>.None, [], Option<string>.None, Option<Uri>.None, [], null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("referencedObjects")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidRoutineType_ThrowsArgumentException()
    {
        Assert.That(
            () => new Routine(RoutineName, Definition, (RoutineType)55, Option<string>.None, [], Option<string>.None, Option<Uri>.None, [], []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineType")
        );
    }

    [Test]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine(RoutineName, null!, RoutineType.Function, Option<string>.None, [], Option<string>.None, Option<Uri>.None, [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [Test]
    public static void Ctor_GivenNullParameters_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine(RoutineName, Definition, RoutineType.Function, Option<string>.None, null!, Option<string>.None, Option<Uri>.None, [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parameters")
        );
    }

    [Test]
    public static void Ctor_GivenNullOverloads_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine(RoutineName, Definition, RoutineType.Function, Option<string>.None, [], Option<string>.None, Option<Uri>.None, null!, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("overloads")
        );
    }

    [Test]
    public static void ParameterCtor_GivenInvalidRoutineParameterDirection_ThrowsArgumentException()
    {
        Assert.That(
            () => new Routine.Parameter(Option<Identifier>.None, "integer", Option<Uri>.None, (RoutineParameterDirection)55, Option<string>.None, 1),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("direction")
        );
    }

    [Test]
    public static void ParameterCtor_GivenNullTypeDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine.Parameter(Option<Identifier>.None, null!, Option<Uri>.None, RoutineParameterDirection.Input, Option<string>.None, 1),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeDefinition")
        );
    }

    [Test]
    public static void OverloadCtor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine.Overload(null!, [], Option<string>.None, Option<Uri>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [Test]
    public static void OverloadCtor_GivenNullParameters_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Routine.Overload(Definition, null!, Option<string>.None, Option<Uri>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parameters")
        );
    }
}
