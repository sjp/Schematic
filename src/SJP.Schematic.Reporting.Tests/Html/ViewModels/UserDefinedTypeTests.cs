using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class UserDefinedTypeTests
{
    private static readonly Identifier TypeName = Identifier.CreateQualifiedIdentifier("test_schema", "test_type");

    [Test]
    public static void Ctor_GivenNullTypeName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedType(null!, UserDefinedTypeKind.Domain, Option<IDbType>.None, Option<Uri>.None, true, Option<string>.None, Option<string>.None, [], [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeName")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidUserDefinedTypeKind_ThrowsArgumentException()
    {
        Assert.That(
            () => new UserDefinedType(TypeName, (UserDefinedTypeKind)55, Option<IDbType>.None, Option<Uri>.None, true, Option<string>.None, Option<string>.None, [], [], []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("kind")
        );
    }

    [Test]
    public static void Ctor_GivenNullEnumValues_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedType(TypeName, UserDefinedTypeKind.Enum, Option<IDbType>.None, Option<Uri>.None, true, Option<string>.None, Option<string>.None, null!, [], []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("enumValues")
        );
    }

    [Test]
    public static void Ctor_GivenNullAttributes_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedType(TypeName, UserDefinedTypeKind.Composite, Option<IDbType>.None, Option<Uri>.None, true, Option<string>.None, Option<string>.None, [], null!, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("attributes")
        );
    }

    [Test]
    public static void Ctor_GivenNullChecks_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedType(TypeName, UserDefinedTypeKind.Domain, Option<IDbType>.None, Option<Uri>.None, true, Option<string>.None, Option<string>.None, [], [], null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("checks")
        );
    }

    [Test]
    public static void AttributeCtor_GivenNullAttributeName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedType.Attribute(null!, 1, true, "integer", Option<Uri>.None, Option<string>.None),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("attributeName")
        );
    }

    [Test]
    public static void CheckCtor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedType.Check(Option<Identifier>.None, null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }
}
