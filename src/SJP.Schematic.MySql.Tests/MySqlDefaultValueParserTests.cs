using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDefaultValueParserTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void Parse_GivenMissingDefinition_ReturnsNone(string definition)
    {
        Assert.That(MySqlDefaultValueParser.Parse(definition, null), OptionIs.None);
    }

    [Test]
    public static void Parse_GivenDefinition_KeepsDefinitionVerbatim()
    {
        const string definition = "unassigned";

        var result = MySqlDefaultValueParser.Parse(definition, null).UnwrapSome();

        Assert.Multiple(() =>
        {
            Assert.That(result.Definition, Is.EqualTo(definition));
            // MySQL has neither named default constraints nor sequences
            Assert.That(result.ConstraintName, OptionIs.None);
            Assert.That(result.SequenceName, OptionIs.None);
        });
    }

    [TestCase("NULL", null, DefaultValueKind.Null)]
    [TestCase("0", null, DefaultValueKind.Literal)]
    // information_schema reports the value of a literal rather than the SQL that produced it
    [TestCase("unassigned", null, DefaultValueKind.Literal)]
    [TestCase("(now())", "DEFAULT_GENERATED", DefaultValueKind.Expression)]
    [TestCase("(uuid())", "DEFAULT_GENERATED", DefaultValueKind.Expression)]
    // a server that does not mark a temporal default as generated still reports it here
    [TestCase("CURRENT_TIMESTAMP", null, DefaultValueKind.Expression)]
    [TestCase("current_timestamp(6)", "on update CURRENT_TIMESTAMP(6)", DefaultValueKind.Expression)]
    public static void Parse_GivenDefinition_ReturnsExpectedKind(string definition, string extraInformation, DefaultValueKind expectedKind)
    {
        var result = MySqlDefaultValueParser.Parse(definition, extraInformation).UnwrapSome();

        Assert.That(result.Kind, Is.EqualTo(expectedKind));
    }
}
