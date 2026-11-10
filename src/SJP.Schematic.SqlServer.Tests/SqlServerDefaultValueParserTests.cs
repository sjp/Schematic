using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerDefaultValueParserTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void Parse_GivenMissingDefinition_ReturnsNone(string definition)
    {
        Assert.That(SqlServerDefaultValueParser.Parse(definition, "df_test"), OptionIs.None);
    }

    [Test]
    public static void Parse_GivenConstraintName_ReturnsName()
    {
        var result = SqlServerDefaultValueParser.Parse("((0))", "df_test").UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.ConstraintName.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("df_test")));
        }
    }

    [TestCase("   ")]
    [TestCase(null)]
    public static void Parse_GivenMissingConstraintName_ReturnsNoneConstraintName(string constraintName)
    {
        var result = SqlServerDefaultValueParser.Parse("((0))", constraintName).UnwrapSome();

        Assert.That(result.ConstraintName, OptionIs.None);
    }

    [TestCase("(NULL)", typeof(DatabaseDefaultValue.Null))]
    [TestCase("(null)", typeof(DatabaseDefaultValue.Null))]
    [TestCase("((0))", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("0", typeof(DatabaseDefaultValue.Literal))]
    [TestCase(" ( ( 0 ) ) ", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("('(a)')", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("((-1.5))", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("('test')", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("(N'test')", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("('o''brien')", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("(getdate())", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("(('a')+('b'))", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("(next value for [dbo].[test_seq])", typeof(DatabaseDefaultValue.SequenceNextValue))]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, Type expectedKind)
    {
        var result = SqlServerDefaultValueParser.Parse(definition, null).UnwrapSome();

        Assert.That(result.Value, Is.TypeOf(expectedKind));
    }

    [TestCase("(NEXT VALUE FOR [dbo].[test_seq])", "dbo", "test_seq")]
    [TestCase("(next value for dbo.test_seq)", "dbo", "test_seq")]
    [TestCase("(next value for [order id seq])", null, "order id seq")]
    public static void Parse_GivenSequenceDefault_ReturnsSequenceName(string definition, string schema, string localName)
    {
        var expectedName = schema != null
            ? Identifier.CreateQualifiedIdentifier(schema, localName)
            : Identifier.CreateQualifiedIdentifier(localName);

        var result = SqlServerDefaultValueParser.Parse(definition, null).UnwrapSome();

        Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.SequenceNextValue>());
        var sequenceDefault = (DatabaseDefaultValue.SequenceNextValue)result.Value!;
        Assert.That(sequenceDefault.SequenceName.UnwrapSome(), Is.EqualTo(expectedName));
    }
}
