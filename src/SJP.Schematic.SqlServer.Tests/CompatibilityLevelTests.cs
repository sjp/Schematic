using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests;

[TestFixture]
internal static class CompatibilityLevelTests
{
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(65)]
    [TestCase(70)]
    [TestCase(100)]
    [TestCase(120)]
    [TestCase(170)]
    [TestCase(20000)]
    public static void Value_PropertyGet_MatchesCtorArgs(int compatLevel)
    {
        var compatiblityLevel = new CompatibilityLevel(compatLevel);

        Assert.That(compatiblityLevel.Value, Is.EqualTo(compatLevel));
    }

    [TestCase(65, SqlServerCompatibilityLevel.SqlServer6_5)]
    [TestCase(70, SqlServerCompatibilityLevel.SqlServer7_0)]
    [TestCase(100, SqlServerCompatibilityLevel.SqlServer2008)]
    [TestCase(120, SqlServerCompatibilityLevel.SqlServer2014)]
    [TestCase(140, SqlServerCompatibilityLevel.SqlServer2017)]
    [TestCase(170, SqlServerCompatibilityLevel.SqlServer2025)]
    [TestCase(-1, SqlServerCompatibilityLevel.Unknown)]
    [TestCase(0, SqlServerCompatibilityLevel.Unknown)]
    [TestCase(9999, SqlServerCompatibilityLevel.Unknown)]
    public static void SqlServerVersion_PropertyGet_MatchesExpectedValues(int compatLevel, SqlServerCompatibilityLevel expectedSqlServerVersion)
    {
        var compatiblityLevel = new CompatibilityLevel(compatLevel);

        Assert.That(compatiblityLevel.SqlServerVersion, Is.EqualTo(expectedSqlServerVersion));
    }
}