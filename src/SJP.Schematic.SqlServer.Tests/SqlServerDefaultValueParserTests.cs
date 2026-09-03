using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests;

[TestFixture]
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

        Assert.Multiple(() =>
        {
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.ConstraintName.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("df_test")));
        });
    }

    [TestCase("   ")]
    [TestCase(null)]
    public static void Parse_GivenMissingConstraintName_ReturnsNoneConstraintName(string constraintName)
    {
        var result = SqlServerDefaultValueParser.Parse("((0))", constraintName).UnwrapSome();

        Assert.That(result.ConstraintName, OptionIs.None);
    }

    [TestCase("(NULL)", DefaultValueKind.Null)]
    [TestCase("(null)", DefaultValueKind.Null)]
    [TestCase("((0))", DefaultValueKind.Literal)]
    [TestCase("((-1.5))", DefaultValueKind.Literal)]
    [TestCase("('test')", DefaultValueKind.Literal)]
    [TestCase("(N'test')", DefaultValueKind.Literal)]
    [TestCase("('o''brien')", DefaultValueKind.Literal)]
    [TestCase("(getdate())", DefaultValueKind.Expression)]
    [TestCase("(('a')+('b'))", DefaultValueKind.Expression)]
    [TestCase("(next value for [dbo].[test_seq])", DefaultValueKind.SequenceNextValue)]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, DefaultValueKind expectedKind)
    {
        var result = SqlServerDefaultValueParser.Parse(definition, null).UnwrapSome();

        Assert.That(result.Kind, Is.EqualTo(expectedKind));
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

        Assert.That(result.SequenceName.UnwrapSome(), Is.EqualTo(expectedName));
    }

    [Test]
    public static void Parse_GivenNonLiteralDefinition_ReturnsNoneSequenceName()
    {
        var result = SqlServerDefaultValueParser.Parse("(getdate())", null).UnwrapSome();

        Assert.That(result.SequenceName, OptionIs.None);
    }
}
