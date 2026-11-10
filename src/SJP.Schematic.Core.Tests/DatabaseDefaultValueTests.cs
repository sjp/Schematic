using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseDefaultValueTests
{
    internal enum DefaultKind
    {
        Unknown,
        Literal,
        Null,
        Expression,
        SequenceNextValue,
    }

    private static DatabaseDefaultValue CreateDefault(DefaultKind kind, string definition) => kind switch
    {
        DefaultKind.Unknown => new DatabaseDefaultValue.Unknown(definition),
        DefaultKind.Literal => new DatabaseDefaultValue.Literal(definition),
        DefaultKind.Null => new DatabaseDefaultValue.Null(definition),
        DefaultKind.Expression => new DatabaseDefaultValue.Expression(definition),
        DefaultKind.SequenceNextValue => new DatabaseDefaultValue.SequenceNextValue(definition),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static DatabaseDefaultValue CreateDefault(DefaultKind kind, string definition, Option<Identifier> constraintName) => kind switch
    {
        DefaultKind.Unknown => new DatabaseDefaultValue.Unknown(definition, constraintName),
        DefaultKind.Literal => new DatabaseDefaultValue.Literal(definition, constraintName),
        DefaultKind.Null => new DatabaseDefaultValue.Null(definition, constraintName),
        DefaultKind.Expression => new DatabaseDefaultValue.Expression(definition, constraintName),
        DefaultKind.SequenceNextValue => new DatabaseDefaultValue.SequenceNextValue(definition, constraintName, Option<Identifier>.None),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    [TestCase(DefaultKind.Unknown)]
    [TestCase(DefaultKind.Literal)]
    [TestCase(DefaultKind.Null)]
    [TestCase(DefaultKind.Expression)]
    [TestCase(DefaultKind.SequenceNextValue)]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException(DefaultKind kind)
    {
        Assert.That(
            () => CreateDefault(kind, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [TestCase(DefaultKind.Unknown, "")]
    [TestCase(DefaultKind.Unknown, "    ")]
    [TestCase(DefaultKind.Literal, "")]
    [TestCase(DefaultKind.Literal, "    ")]
    [TestCase(DefaultKind.Null, "")]
    [TestCase(DefaultKind.Null, "    ")]
    [TestCase(DefaultKind.Expression, "")]
    [TestCase(DefaultKind.Expression, "    ")]
    [TestCase(DefaultKind.SequenceNextValue, "")]
    [TestCase(DefaultKind.SequenceNextValue, "    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(DefaultKind kind, string definition)
    {
        Assert.That(
            () => CreateDefault(kind, definition),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("definition")
        );
    }

    [TestCase(DefaultKind.Unknown, typeof(DatabaseDefaultValue.Unknown))]
    [TestCase(DefaultKind.Literal, typeof(DatabaseDefaultValue.Literal))]
    [TestCase(DefaultKind.Null, typeof(DatabaseDefaultValue.Null))]
    [TestCase(DefaultKind.Expression, typeof(DatabaseDefaultValue.Expression))]
    [TestCase(DefaultKind.SequenceNextValue, typeof(DatabaseDefaultValue.SequenceNextValue))]
    public static void Value_PropertyGet_IsTheConstructedCase(DefaultKind kind, Type expectedType)
    {
        var defaultValue = CreateDefault(kind, "0");

        Assert.That(defaultValue.Value, Is.TypeOf(expectedType));
    }

    [TestCase(DefaultKind.Unknown)]
    [TestCase(DefaultKind.Literal)]
    [TestCase(DefaultKind.Null)]
    [TestCase(DefaultKind.Expression)]
    [TestCase(DefaultKind.SequenceNextValue)]
    public static void Definition_PropertyGet_ReturnsCtorArg(DefaultKind kind)
    {
        const string definition = "((0))";

        var defaultValue = CreateDefault(kind, definition);

        Assert.That(defaultValue.Definition, Is.EqualTo(definition));
    }

    [TestCase(DefaultKind.Unknown)]
    [TestCase(DefaultKind.Literal)]
    [TestCase(DefaultKind.Null)]
    [TestCase(DefaultKind.Expression)]
    [TestCase(DefaultKind.SequenceNextValue)]
    public static void ConstraintName_GivenNoName_ReturnsNone(DefaultKind kind)
    {
        var defaultValue = CreateDefault(kind, "0");

        Assert.That(defaultValue.ConstraintName, OptionIs.None);
    }

    [TestCase(DefaultKind.Unknown)]
    [TestCase(DefaultKind.Literal)]
    [TestCase(DefaultKind.Null)]
    [TestCase(DefaultKind.Expression)]
    [TestCase(DefaultKind.SequenceNextValue)]
    public static void ConstraintName_GivenQualifiedName_ReturnsLocalNameOnly(DefaultKind kind)
    {
        var constraintName = Identifier.CreateQualifiedIdentifier("test_schema", "df_test");

        var defaultValue = CreateDefault(kind, "((0))", constraintName);

        Assert.That(defaultValue.ConstraintName.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("df_test")));
    }

    [Test]
    public static void SequenceName_PropertyGet_ReturnsCtorArg()
    {
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");

        var defaultValue = new DatabaseDefaultValue.SequenceNextValue(
            "next value for [test_schema].[test_seq]",
            Option<Identifier>.None,
            sequenceName);

        Assert.That(defaultValue.SequenceName.UnwrapSome(), Is.EqualTo(sequenceName));
    }

    [Test]
    public static void SequenceName_WhenNotProvidedInCtor_ReturnsNone()
    {
        var defaultValue = new DatabaseDefaultValue.SequenceNextValue("next value for [test_schema].[test_seq]");

        Assert.That(defaultValue.SequenceName, OptionIs.None);
    }

    [TestCase(DefaultKind.Unknown)]
    [TestCase(DefaultKind.Literal)]
    [TestCase(DefaultKind.Null)]
    [TestCase(DefaultKind.Expression)]
    [TestCase(DefaultKind.SequenceNextValue)]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(DefaultKind kind)
    {
        var defaultValue = CreateDefault(kind, "((0))", Option<Identifier>.Some("df_test"));

        Assert.That(defaultValue.ToString(), Is.EqualTo("Default: df_test = ((0))"));
    }

    [TestCase(DefaultKind.Unknown)]
    [TestCase(DefaultKind.Literal)]
    [TestCase(DefaultKind.Null)]
    [TestCase(DefaultKind.Expression)]
    [TestCase(DefaultKind.SequenceNextValue)]
    public static void ToString_GivenNoConstraintName_ReturnsExpectedValues(DefaultKind kind)
    {
        var defaultValue = CreateDefault(kind, "((0))");

        Assert.That(defaultValue.ToString(), Is.EqualTo("Default: ((0))"));
    }

    [Test]
    public static void Definition_WhenDefaultValue_Throws()
    {
        var defaultValue = default(DatabaseDefaultValue);

        Assert.That(() => defaultValue.Definition, Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public static void ToString_WhenDefaultValue_ReturnsEmptyString()
    {
        var defaultValue = default(DatabaseDefaultValue);

        Assert.That(defaultValue.ToString(), Is.Empty);
    }
}
