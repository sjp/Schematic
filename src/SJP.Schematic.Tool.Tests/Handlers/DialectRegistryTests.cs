#nullable enable
using System;
using NUnit.Framework;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Tests.Handlers;

[TestFixture]
internal static class DialectRegistryTests
{
    [TestCase("sqlserver")]
    [TestCase("postgresql")]
    [TestCase("mysql")]
    [TestCase("oracle")]
    [TestCase("sqlite")]
    public static void DialectNames_PropertyGet_ContainsSupportedDialect(string dialect)
    {
        Assert.That(DialectRegistry.DialectNames, Contains.Item(dialect));
    }

    [TestCase("sqlserver", "Database", false)]
    [TestCase("postgresql", "Database", false)]
    [TestCase("mysql", "Database", false)]
    [TestCase("oracle", "Service name", false)]
    [TestCase("sqlite", "Database", true)]
    public static void Get_GivenSupportedDialect_DescribesItsConnectionDetails(string dialect, string expectedLabel, bool expectedFileBased)
    {
        var descriptor = DialectRegistry.Get(dialect);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(descriptor.Name, Is.EqualTo(dialect));
            Assert.That(descriptor.DatabaseLabel, Is.EqualTo(expectedLabel));
            Assert.That(descriptor.IsFileBased, Is.EqualTo(expectedFileBased));
        }
    }

    [Test]
    public static void Get_GivenDialectInDifferentCase_ReturnsDescriptor()
    {
        var descriptor = DialectRegistry.Get("SqlServer");

        Assert.That(descriptor.Name, Is.EqualTo("sqlserver"));
    }

    [TestCase((string?)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Get_GivenNullOrWhiteSpaceDialect_ThrowsArgumentException(string? dialect)
    {
        Assert.That(() => DialectRegistry.Get(dialect!), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void Get_GivenUnsupportedDialect_ThrowsNotSupportedException()
    {
        Assert.That(() => DialectRegistry.Get("db2"), Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public static void TryGet_GivenUnsupportedDialect_ReturnsFalse()
    {
        var result = DialectRegistry.TryGet("db2", out var descriptor);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(descriptor, Is.Null);
        }
    }

    [TestCase((string?)null)]
    [TestCase("   ")]
    public static void TryGet_GivenNullOrWhiteSpaceDialect_ReturnsFalse(string? dialect)
    {
        var result = DialectRegistry.TryGet(dialect, out _);

        Assert.That(result, Is.False);
    }

    [TestCase("sqlserver", typeof(SqlServer.SqlServerDialect))]
    [TestCase("postgresql", typeof(PostgreSql.PostgreSqlDialect))]
    [TestCase("mysql", typeof(MySql.MySqlDialect))]
    [TestCase("oracle", typeof(Oracle.OracleDialect))]
    [TestCase("sqlite", typeof(Sqlite.SqliteDialect))]
    public static void CreateDialect_GivenSupportedDialect_ReturnsExpectedDialect(string dialect, Type expectedType)
    {
        var result = DialectRegistry.Get(dialect).CreateDialect();

        Assert.That(result, Is.InstanceOf(expectedType));
    }
}
