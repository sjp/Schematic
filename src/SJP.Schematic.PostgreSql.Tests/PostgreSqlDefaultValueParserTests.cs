using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlDefaultValueParserTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void Parse_GivenMissingDefinition_ReturnsNone(string definition)
    {
        Assert.That(PostgreSqlDefaultValueParser.Parse(definition), OptionIs.None);
    }

    [Test]
    public static void Parse_GivenDefinition_KeepsDefinitionVerbatim()
    {
        const string definition = "'unassigned'::character varying";

        var result = PostgreSqlDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.Multiple(() =>
        {
            Assert.That(result.Definition, Is.EqualTo(definition));
            // PostgreSQL has no named default constraints
            Assert.That(result.ConstraintName, OptionIs.None);
        });
    }

    [TestCase("NULL", DefaultValueKind.Null)]
    [TestCase("NULL::character varying", DefaultValueKind.Null)]
    [TestCase("0", DefaultValueKind.Literal)]
    [TestCase("-1.5", DefaultValueKind.Literal)]
    [TestCase("true", DefaultValueKind.Literal)]
    [TestCase("false", DefaultValueKind.Literal)]
    [TestCase("'test'::text", DefaultValueKind.Literal)]
    [TestCase("'test'::character varying(50)", DefaultValueKind.Literal)]
    [TestCase("'{}'::jsonb", DefaultValueKind.Literal)]
    [TestCase("'{}'::text[]", DefaultValueKind.Literal)]
    [TestCase("now()", DefaultValueKind.Expression)]
    [TestCase("CURRENT_TIMESTAMP", DefaultValueKind.Expression)]
    [TestCase("(a)::integer + (b)::integer", DefaultValueKind.Expression)]
    [TestCase("nextval('test_seq'::regclass)", DefaultValueKind.SequenceNextValue)]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, DefaultValueKind expectedKind)
    {
        var result = PostgreSqlDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.That(result.Kind, Is.EqualTo(expectedKind));
    }

    [TestCase("nextval('test_seq'::regclass)", null, "test_seq")]
    [TestCase("nextval('public.test_seq'::regclass)", "public", "test_seq")]
    [TestCase("nextval('public.\"Test Seq\"'::regclass)", "public", "Test Seq")]
    [TestCase("nextval('test_seq')", null, "test_seq")]
    public static void Parse_GivenSequenceDefault_ReturnsSequenceName(string definition, string schema, string localName)
    {
        var expectedName = schema != null
            ? Identifier.CreateQualifiedIdentifier(schema, localName)
            : Identifier.CreateQualifiedIdentifier(localName);

        var result = PostgreSqlDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.That(result.SequenceName.UnwrapSome(), Is.EqualTo(expectedName));
    }
}
