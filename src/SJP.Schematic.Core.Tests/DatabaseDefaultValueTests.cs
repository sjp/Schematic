using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseDefaultValueTests
{
    [Test]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseDefaultValue(null), Throws.ArgumentNullException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Assert.That(() => new DatabaseDefaultValue(definition), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenInvalidKind_ThrowsArgumentException()
    {
        const DefaultValueKind kind = (DefaultValueKind)55;

        Assert.That(() => new DatabaseDefaultValue("0", kind), Throws.ArgumentException);
    }

    [Test]
    public static void Definition_PropertyGet_ReturnsCtorArg()
    {
        const string definition = "((0))";

        var defaultValue = new DatabaseDefaultValue(definition);

        Assert.That(defaultValue.Definition, Is.EqualTo(definition));
    }

    [Test]
    public static void Kind_GivenNoKind_ReturnsUnknown()
    {
        var defaultValue = new DatabaseDefaultValue("0");

        Assert.That(defaultValue.Kind, Is.EqualTo(DefaultValueKind.Unknown));
    }

    [Test]
    public static void Kind_PropertyGet_ReturnsCtorArg()
    {
        var defaultValue = new DatabaseDefaultValue("0", DefaultValueKind.Literal);

        Assert.That(defaultValue.Kind, Is.EqualTo(DefaultValueKind.Literal));
    }

    [Test]
    public static void ConstraintName_GivenNoName_ReturnsNone()
    {
        var defaultValue = new DatabaseDefaultValue("0", DefaultValueKind.Literal);

        Assert.That(defaultValue.ConstraintName, OptionIs.None);
    }

    [Test]
    public static void ConstraintName_GivenQualifiedName_ReturnsLocalNameOnly()
    {
        var constraintName = Identifier.CreateQualifiedIdentifier("test_schema", "df_test");

        var defaultValue = new DatabaseDefaultValue(
            "((0))",
            DefaultValueKind.Literal,
            Option<Identifier>.Some(constraintName),
            Option<Identifier>.None);

        Assert.That(defaultValue.ConstraintName.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("df_test")));
    }

    [Test]
    public static void SequenceName_GivenSequenceNextValueKind_ReturnsCtorArg()
    {
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");

        var defaultValue = new DatabaseDefaultValue(
            "next value for [test_schema].[test_seq]",
            DefaultValueKind.SequenceNextValue,
            Option<Identifier>.None,
            Option<Identifier>.Some(sequenceName));

        Assert.That(defaultValue.SequenceName.UnwrapSome(), Is.EqualTo(sequenceName));
    }

    [Test]
    public static void SequenceName_GivenNonSequenceKind_ReturnsNone()
    {
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");

        var defaultValue = new DatabaseDefaultValue(
            "0",
            DefaultValueKind.Literal,
            Option<Identifier>.None,
            Option<Identifier>.Some(sequenceName));

        Assert.That(defaultValue.SequenceName, OptionIs.None);
    }

    [Test]
    public static void ToString_WhenInvoked_ReturnsExpectedValues()
    {
        const string definition = "((0))";
        var defaultValue = new DatabaseDefaultValue(
            definition,
            DefaultValueKind.Literal,
            Option<Identifier>.Some("df_test"),
            Option<Identifier>.None);

        Assert.That(defaultValue.ToString(), Is.EqualTo("Default: df_test = ((0))"));
    }

    [Test]
    public static void ToString_GivenNoConstraintName_ReturnsExpectedValues()
    {
        var defaultValue = new DatabaseDefaultValue("((0))", DefaultValueKind.Literal);

        Assert.That(defaultValue.ToString(), Is.EqualTo("Default: ((0))"));
    }
}
