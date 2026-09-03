using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleCatalogMapperTests
{
    [TestCase("NO ACTION", ReferentialAction.NoAction)]
    [TestCase("RESTRICT", ReferentialAction.Restrict)]
    [TestCase("CASCADE", ReferentialAction.Cascade)]
    [TestCase("SET NULL", ReferentialAction.SetNull)]
    [TestCase("SET DEFAULT", ReferentialAction.SetDefault)]
    [TestCase("set null", ReferentialAction.SetNull)]
    public static void GetReferentialAction_GivenKnownDeleteRule_ReturnsExpectedAction(string deleteRule, ReferentialAction expected)
    {
        Assert.That(OracleCatalogMapper.GetReferentialAction(deleteRule), Is.EqualTo(expected));
    }

    [Test]
    public static void GetReferentialAction_GivenNullDeleteRule_ReturnsNoAction()
    {
        Assert.That(OracleCatalogMapper.GetReferentialAction(null), Is.EqualTo(ReferentialAction.NoAction));
    }

    [TestCase("")]
    [TestCase("    ")]
    [TestCase("SOMETHING UNEXPECTED")]
    public static void GetReferentialAction_GivenUnknownDeleteRule_ReturnsNoAction(string deleteRule)
    {
        Assert.That(OracleCatalogMapper.GetReferentialAction(deleteRule), Is.EqualTo(ReferentialAction.NoAction));
    }
}
