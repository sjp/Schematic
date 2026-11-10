using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Definition, Is.EqualTo(definition));
            // Oracle has no named default constraints
            Assert.That(result.ConstraintName, OptionIs.None);
        }
    }

    [TestCase("NULL", typeof(DatabaseDefaultValue.Null))]
    [TestCase("null ", typeof(DatabaseDefaultValue.Null))]
    [TestCase("0", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("-1.5", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'test'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'o''brien'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("SYS_GUID()", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("SYSDATE", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("a.b + c.d", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("TEST_SEQ.NEXTVAL", typeof(DatabaseDefaultValue.SequenceNextValue))]
    [TestCase("\"HR\".\"TEST_SEQ\".\"NEXTVAL\"", typeof(DatabaseDefaultValue.SequenceNextValue))]
    [TestCase("test_seq.NextVal ", typeof(DatabaseDefaultValue.SequenceNextValue))]
    // a quoted pseudocolumn name is case sensitive
    [TestCase("\"HR\".\"TEST_SEQ\".\"nextval\"", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("NEXTVAL", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("TEST_SEQ.NEXTVAL + 1", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("1 + TEST_SEQ.NEXTVAL", typeof(DatabaseDefaultValue.Expression))]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, Type expectedKind)
    {
        var result = OracleDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.That(result.Value, Is.TypeOf(expectedKind));
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

        Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.SequenceNextValue>());
        var sequenceDefault = (DatabaseDefaultValue.SequenceNextValue)result.Value!;
        Assert.That(sequenceDefault.SequenceName.UnwrapSome(), Is.EqualTo(expectedName));
    }
}
