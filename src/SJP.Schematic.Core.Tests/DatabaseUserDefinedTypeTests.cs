using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseUserDefinedTypeTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType(null, UserDefinedTypeKind.Alias, Option<IDbType>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidKind_ThrowsArgumentException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", (UserDefinedTypeKind)55, Option<IDbType>.None),
            Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNullEnumValues_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Enum, Option<IDbType>.None, null, [], [], true, Option<string>.None, Option<string>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEnumValuesWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Enum, Option<IDbType>.None, [null], [], [], true, Option<string>.None, Option<string>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullAttributes_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Composite, Option<IDbType>.None, [], null, [], true, Option<string>.None, Option<string>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenAttributesWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Composite, Option<IDbType>.None, [], [null], [], true, Option<string>.None, Option<string>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullChecks_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Domain, Option<IDbType>.None, [], [], null, true, Option<string>.None, Option<string>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenChecksWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Domain, Option<IDbType>.None, [], [], [null], true, Option<string>.None, Option<string>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        const string typeName = "test_type";
        var userDefinedType = new DatabaseUserDefinedType(typeName, UserDefinedTypeKind.Alias, Option<IDbType>.None);

        Assert.That(userDefinedType.Name.LocalName, Is.EqualTo(typeName));
    }

    [Test]
    public static void SimpleCtor_WhenInvoked_LeavesEveryCollectionEmptyAndTypeNullable()
    {
        var userDefinedType = new DatabaseUserDefinedType("test_type", UserDefinedTypeKind.Alias, Option<IDbType>.None);

        Assert.Multiple(() =>
        {
            Assert.That(userDefinedType.Kind, Is.EqualTo(UserDefinedTypeKind.Alias));
            Assert.That(userDefinedType.BaseType, OptionIs.None);
            Assert.That(userDefinedType.EnumValues, Is.Empty);
            Assert.That(userDefinedType.Attributes, Is.Empty);
            Assert.That(userDefinedType.Checks, Is.Empty);
            Assert.That(userDefinedType.IsNullable, Is.True);
            Assert.That(userDefinedType.DefaultValue, OptionIs.None);
            Assert.That(userDefinedType.Definition, OptionIs.None);
        });
    }

    [Test]
    public static void Ctor_GivenFullDefinition_PropertiesEqualCtorArgs()
    {
        var baseType = TestDbTypes.BigInteger;
        var attribute = new DatabaseColumn("attr", baseType, true, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "([value] > 0)", true);
        IReadOnlyList<string> enumValues = ["a", "b"];

        var userDefinedType = new DatabaseUserDefinedType(
            "test_type",
            UserDefinedTypeKind.Domain,
            Option<IDbType>.Some(baseType),
            enumValues,
            [attribute],
            [check],
            false,
            Option<string>.Some("'abc'"),
            Option<string>.Some("create type ..."));

        Assert.Multiple(() =>
        {
            Assert.That(userDefinedType.Kind, Is.EqualTo(UserDefinedTypeKind.Domain));
            Assert.That(userDefinedType.BaseType.UnwrapSome(), Is.EqualTo(baseType));
            Assert.That(userDefinedType.EnumValues, Is.EqualTo(enumValues));
            Assert.That(userDefinedType.Attributes, Is.EqualTo(new[] { attribute }));
            Assert.That(userDefinedType.Checks, Is.EqualTo(new[] { check }));
            Assert.That(userDefinedType.IsNullable, Is.False);
            Assert.That(userDefinedType.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
            Assert.That(userDefinedType.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        });
    }

    [TestCase("", "test_type", "Type: test_type")]
    [TestCase("test_schema", "test_type", "Type: test_schema.test_type")]
    public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
    {
        var typeName = Identifier.CreateQualifiedIdentifier(schema, localName);
        var userDefinedType = new DatabaseUserDefinedType(typeName, UserDefinedTypeKind.Alias, Option<IDbType>.None);

        Assert.That(userDefinedType.ToString(), Is.EqualTo(expectedOutput));
    }
}
