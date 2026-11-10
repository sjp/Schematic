using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests;

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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Definition, Is.EqualTo(definition));
            // PostgreSQL has no named default constraints
            Assert.That(result.ConstraintName, OptionIs.None);
        }
    }

    [TestCase("NULL", typeof(DatabaseDefaultValue.Null))]
    [TestCase("NULL::character varying", typeof(DatabaseDefaultValue.Null))]
    [TestCase("0", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("-1.5", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("true", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("false", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'test'::text", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'test'::character varying(50)", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'{}'::jsonb", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'{}'::text[]", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'happy'::app.mood", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'happy'::\"Mood\"", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'happy'::app.\"Mood\"", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'happy'::\"App\".\"Mood\"", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'happy'::\"Odd \"\"Mood\"\" :: Name\"", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'{happy}'::app.mood[]", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'{happy}'::app.\"Mood\"[]", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("('happy'::app.\"Mood\" || 'x'::text)", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("now()", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("CURRENT_TIMESTAMP", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("(a)::integer + (b)::integer", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("nextval('test_seq'::regclass)", typeof(DatabaseDefaultValue.SequenceNextValue))]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, Type expectedKind)
    {
        var result = PostgreSqlDefaultValueParser.Parse(definition).UnwrapSome();

        Assert.That(result.Value, Is.TypeOf(expectedKind));
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

        Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.SequenceNextValue>());
        var sequenceDefault = (DatabaseDefaultValue.SequenceNextValue)result.Value!;
        Assert.That(sequenceDefault.SequenceName.UnwrapSome(), Is.EqualTo(expectedName));
    }
}
