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
            Assert.That(result.Kind, Is.EqualTo(DefaultValueKind.Literal));
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
            // MySQL has neither named default constraints nor sequences
            Assert.That(result.ConstraintName, OptionIs.None);
            Assert.That(result.SequenceName, OptionIs.None);
        }
    }

    [TestCase("0", null, DefaultValueKind.Literal)]
    // information_schema reports the value of a literal rather than the SQL that produced it
    [TestCase("unassigned", null, DefaultValueKind.Literal)]
    [TestCase("(now())", "DEFAULT_GENERATED", DefaultValueKind.Expression)]
    [TestCase("(uuid())", "DEFAULT_GENERATED", DefaultValueKind.Expression)]
    // a server that does not mark a temporal default as generated still reports it here
    [TestCase("CURRENT_TIMESTAMP", null, DefaultValueKind.Expression)]
    [TestCase("current_timestamp(6)", "on update CURRENT_TIMESTAMP(6)", DefaultValueKind.Expression)]
    public static void Parse_GivenMySqlDefinition_ReturnsExpectedKind(string definition, string extraInformation, DefaultValueKind expectedKind)
    {
        var result = MySqlDefaultValueParser.Parse(definition, extraInformation, false).UnwrapSome();

        Assert.That(result.Kind, Is.EqualTo(expectedKind));
    }

    // MariaDB never marks a default as generated: literals are quoted or numeric, anything else is SQL
    [TestCase("'abc'", DefaultValueKind.Literal)]
    [TestCase("''", DefaultValueKind.Literal)]
    [TestCase("'NULL'", DefaultValueKind.Literal)]
    [TestCase("'2020-01-01'", DefaultValueKind.Literal)]
    [TestCase("5", DefaultValueKind.Literal)]
    [TestCase("-1.50", DefaultValueKind.Literal)]
    [TestCase("1000", DefaultValueKind.Literal)]
    [TestCase("-0.001", DefaultValueKind.Literal)]
    [TestCase("1e3", DefaultValueKind.Literal)]
    [TestCase("b'101'", DefaultValueKind.Literal)]
    [TestCase("x'41'", DefaultValueKind.Literal)]
    [TestCase("0x41", DefaultValueKind.Literal)]
    [TestCase("current_timestamp()", DefaultValueKind.Expression)]
    [TestCase("current_timestamp(6)", DefaultValueKind.Expression)]
    [TestCase("uuid()", DefaultValueKind.Expression)]
    [TestCase("curtime()", DefaultValueKind.Expression)]
    [TestCase("(1 + 1)", DefaultValueKind.Expression)]
    [TestCase("(rand() * 0)", DefaultValueKind.Expression)]
    [TestCase("concat('a','b')", DefaultValueKind.Expression)]
    public static void Parse_GivenMariaDbDefinition_ReturnsExpectedKind(string definition, DefaultValueKind expectedKind)
    {
        var result = MySqlDefaultValueParser.Parse(definition, null, true).UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Kind, Is.EqualTo(expectedKind));
            Assert.That(result.Definition, Is.EqualTo(definition));
        }
    }
}
