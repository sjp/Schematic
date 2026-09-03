using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDefaultValueParserTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void Parse_GivenMissingDefinition_ReturnsNone(string definition)
    {
        Assert.That(OracleDefaultValueParser.Parse(definition), OptionIs.None);
    }

    [Test]
    public static void Parse_GivenDefinition_KeepsDefinitionVerbatim()
    {
        // DATA_DEFAULT keeps the whitespace that followed the clause in the DDL
        const string definition = "'unassigned' ";

        var result = OracleDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.Multiple(() =>
        {
            Assert.That(result.Definition, Is.EqualTo(definition));
            // Oracle has no named default constraints
            Assert.That(result.ConstraintName, OptionIs.None);
        });
    }

    [TestCase("NULL", DefaultValueKind.Null)]
    [TestCase("null ", DefaultValueKind.Null)]
    [TestCase("0", DefaultValueKind.Literal)]
    [TestCase("-1.5", DefaultValueKind.Literal)]
    [TestCase("'test'", DefaultValueKind.Literal)]
    [TestCase("'o''brien'", DefaultValueKind.Literal)]
    [TestCase("SYS_GUID()", DefaultValueKind.Expression)]
    [TestCase("SYSDATE", DefaultValueKind.Expression)]
    [TestCase("a.b + c.d", DefaultValueKind.Expression)]
    [TestCase("TEST_SEQ.NEXTVAL", DefaultValueKind.SequenceNextValue)]
    [TestCase("\"HR\".\"TEST_SEQ\".\"NEXTVAL\"", DefaultValueKind.SequenceNextValue)]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, DefaultValueKind expectedKind)
    {
        var result = OracleDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.That(result.Kind, Is.EqualTo(expectedKind));
    }

    [TestCase("TEST_SEQ.NEXTVAL", null, "TEST_SEQ")]
    // an unquoted name is folded to upper case, which is how the rest of the catalog reports it
    [TestCase("test_seq.nextval", null, "TEST_SEQ")]
    [TestCase("\"HR\".\"TEST_SEQ\".\"NEXTVAL\"", "HR", "TEST_SEQ")]
    [TestCase("\"HR\".\"Test Seq\".nextval", "HR", "Test Seq")]
    public static void Parse_GivenSequenceDefault_ReturnsSequenceName(string definition, string schema, string localName)
    {
        var expectedName = schema != null
            ? Identifier.CreateQualifiedIdentifier(schema, localName)
            : Identifier.CreateQualifiedIdentifier(localName);

        var result = OracleDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.That(result.SequenceName.UnwrapSome(), Is.EqualTo(expectedName));
    }
}
