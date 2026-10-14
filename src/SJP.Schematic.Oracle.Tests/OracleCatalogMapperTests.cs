using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests;

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

    [TestCase("NUMBER", 10, 2, 10, 2)]
    [TestCase("NUMBER", 5, 0, 5, 0)]
    [TestCase("FLOAT", 126, 0, 126, 0)]
    [TestCase("INTERVAL DAY(2) TO SECOND(6)", 2, 6, 2, 6)]
    public static void GetNumericPrecision_GivenPrecision_ReturnsPrecisionAndScale(string typeName, int precision, int scale, int expectedPrecision, int expectedScale)
    {
        var result = OracleCatalogMapper.GetNumericPrecision(typeName, precision, scale);

        Assert.That(result, OptionIs.Some);
        var numericPrecision = result.UnwrapSome();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(expectedPrecision));
            Assert.That(numericPrecision.Scale, Is.EqualTo(expectedScale));
        }
    }

    [Test]
    public static void GetNumericPrecision_GivenNegativeScale_ClampsScaleToZero()
    {
        var result = OracleCatalogMapper.GetNumericPrecision("NUMBER", 5, -2);

        Assert.That(result, OptionIs.Some);
        var numericPrecision = result.UnwrapSome();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(5));
            Assert.That(numericPrecision.Scale, Is.Zero);
        }
    }

    [Test]
    public static void GetNumericPrecision_GivenNumberWithScaleButNoPrecision_ReturnsMaximumPrecision()
    {
        var result = OracleCatalogMapper.GetNumericPrecision("NUMBER", 0, 2);

        Assert.That(result, OptionIs.Some);
        var numericPrecision = result.UnwrapSome();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numericPrecision.Precision, Is.EqualTo(38));
            Assert.That(numericPrecision.Scale, Is.EqualTo(2));
        }
    }

    [TestCase("TIMESTAMP(6)", 0, 6)]
    [TestCase("TIMESTAMP(3) WITH TIME ZONE", 0, 3)]
    [TestCase("TIMESTAMP(9) WITH LOCAL TIME ZONE", 0, 9)]
    [TestCase("INTERVAL DAY(0) TO SECOND(6)", 0, 6)]
    [TestCase("NUMBER", 0, 0)]
    [TestCase("NUMBER", 0, -2)]
    [TestCase("VARCHAR2", 0, 0)]
    [TestCase(null, 0, 4)]
    public static void GetNumericPrecision_GivenNoPrecisionForType_ReturnsNone(string typeName, int precision, int scale)
    {
        var result = OracleCatalogMapper.GetNumericPrecision(typeName, precision, scale);

        Assert.That(result, OptionIs.None);
    }
}
