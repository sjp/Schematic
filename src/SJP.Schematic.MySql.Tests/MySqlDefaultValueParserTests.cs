using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests;

internal static class MySqlDefaultValueParserTests
{
    [TestCase(null, false)]
    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("   ", true)]
    // MariaDB reports a column with no default as the text 'NULL' rather than as SQL NULL
    [TestCase("NULL", true)]
    [TestCase("null", true)]
    [TestCase("NULL", false)]
    public static void Parse_GivenMissingDefinition_ReturnsNone(string definition, bool isMariaDb)
    {
        Assert.That(MySqlDefaultValueParser.Parse(definition, null, isMariaDb), OptionIs.None);
    }

    // MySQL reports an empty or whitespace string default unquoted, as the string itself
    [TestCase("", "''")]
    [TestCase("   ", "'   '")]
    public static void Parse_GivenBlankMySqlDefinition_ReturnsQuotedLiteral(string definition, string expectedDefinition)
    {
        var result = MySqlDefaultValueParser.Parse(definition, null, false).UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Definition, Is.EqualTo(expectedDefinition));
            Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.Literal>());
        }
    }

    [Test]
    public static void Parse_GivenDefinition_KeepsDefinitionVerbatim()
    {
        const string definition = "unassigned";

        var result = MySqlDefaultValueParser.Parse(definition, null, false).UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Definition, Is.EqualTo(definition));
            // MySQL has no named default constraints
            Assert.That(result.ConstraintName, OptionIs.None);
        }
    }

    [TestCase("0", null, typeof(DatabaseDefaultValue.Literal))]
    // information_schema reports the value of a literal rather than the SQL that produced it
    [TestCase("unassigned", null, typeof(DatabaseDefaultValue.Literal))]
    [TestCase("(now())", "DEFAULT_GENERATED", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("(uuid())", "DEFAULT_GENERATED", typeof(DatabaseDefaultValue.Expression))]
    // a server that does not mark a temporal default as generated still reports it here
    [TestCase("CURRENT_TIMESTAMP", null, typeof(DatabaseDefaultValue.Expression))]
    [TestCase("current_timestamp(6)", "on update CURRENT_TIMESTAMP(6)", typeof(DatabaseDefaultValue.Expression))]
    public static void Parse_GivenMySqlDefinition_ReturnsExpectedKind(string definition, string extraInformation, Type expectedKind)
    {
        var result = MySqlDefaultValueParser.Parse(definition, extraInformation, false).UnwrapSome();

        Assert.That(result.Value, Is.TypeOf(expectedKind));
    }

    // MariaDB never marks a default as generated: literals are quoted or numeric, anything else is SQL
    [TestCase("'abc'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("''", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'NULL'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("'2020-01-01'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("5", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("-1.50", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("1000", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("-0.001", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("1e3", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("b'101'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("x'41'", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("0x41", typeof(DatabaseDefaultValue.Literal))]
    [TestCase("current_timestamp()", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("current_timestamp(6)", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("uuid()", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("curtime()", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("(1 + 1)", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("(rand() * 0)", typeof(DatabaseDefaultValue.Expression))]
    [TestCase("concat('a','b')", typeof(DatabaseDefaultValue.Expression))]
    public static void Parse_GivenMariaDbDefinition_ReturnsExpectedKind(string definition, Type expectedKind)
    {
        var result = MySqlDefaultValueParser.Parse(definition, null, true).UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf(expectedKind));
            Assert.That(result.Definition, Is.EqualTo(definition));
        }
    }
}
