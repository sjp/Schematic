using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests;

internal static class SqliteTypeAffinityParserTests
{
    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqliteTypeAffinityParser(), Throws.Nothing);
    }

    // the rules are applied in order, so a name matching more than one rule takes the first affinity
    [TestCase("INT", SqliteTypeAffinity.Integer)]
    [TestCase("integer", SqliteTypeAffinity.Integer)]
    [TestCase("TINYINT", SqliteTypeAffinity.Integer)]
    [TestCase("UNSIGNED BIG INT", SqliteTypeAffinity.Integer)]
    [TestCase("CHARINT", SqliteTypeAffinity.Integer)]
    [TestCase("FLOATING POINT", SqliteTypeAffinity.Integer)]
    [TestCase("VARCHAR(255)", SqliteTypeAffinity.Text)]
    [TestCase("nchar(55)", SqliteTypeAffinity.Text)]
    [TestCase("CLOB", SqliteTypeAffinity.Text)]
    [TestCase("TEXT", SqliteTypeAffinity.Text)]
    [TestCase("TEXTBLOB", SqliteTypeAffinity.Text)]
    [TestCase("BLOB", SqliteTypeAffinity.Blob)]
    [TestCase("blob", SqliteTypeAffinity.Blob)]
    [TestCase("BLOBREAL", SqliteTypeAffinity.Blob)]
    [TestCase("REAL", SqliteTypeAffinity.Real)]
    [TestCase("DOUBLE PRECISION", SqliteTypeAffinity.Real)]
    [TestCase("float", SqliteTypeAffinity.Real)]
    [TestCase("NUMERIC", SqliteTypeAffinity.Numeric)]
    [TestCase("DECIMAL(10, 5)", SqliteTypeAffinity.Numeric)]
    [TestCase("BOOLEAN", SqliteTypeAffinity.Numeric)]
    [TestCase("DATETIME", SqliteTypeAffinity.Numeric)]
    [TestCase("STRING", SqliteTypeAffinity.Numeric)]
    [TestCase("ANY", SqliteTypeAffinity.Numeric)]
    public static void ParseTypeName_GivenTypeName_ReturnsExpectedAffinity(string typeName, SqliteTypeAffinity expected)
    {
        var parser = new SqliteTypeAffinityParser();

        Assert.That(parser.ParseTypeName(typeName), Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void ParseTypeName_GivenMissingTypeName_ReturnsBlobAffinity(string typeName)
    {
        var parser = new SqliteTypeAffinityParser();

        Assert.That(parser.ParseTypeName(typeName), Is.EqualTo(SqliteTypeAffinity.Blob));
    }
}
