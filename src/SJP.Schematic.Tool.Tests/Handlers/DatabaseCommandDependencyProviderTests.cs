#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.DataAccess;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Tests.Handlers;

[TestFixture]
internal static class DatabaseCommandDependencyProviderTests
{
    private static DatabaseCommandDependencyProvider CreateProvider(string? dialect, string? connectionString)
    {
        var values = new Dictionary<string, string?>();
        if (dialect != null)
            values["Dialect"] = dialect;
        if (connectionString != null)
            values["ConnectionStrings:Schematic"] = connectionString;

        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new DatabaseCommandDependencyProvider(config);
    }

    [Test]
    public static void Ctor_GivenNullConfiguration_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseCommandDependencyProvider(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetConnectionString_GivenConfiguredValue_ReturnsValue()
    {
        var provider = CreateProvider("sqlite", "Data Source=:memory:");

        Assert.That(provider.GetConnectionString(), Is.EqualTo("Data Source=:memory:"));
    }

    [Test]
    public static void GetConnectionString_GivenMissingValue_ThrowsInvalidOperationException()
    {
        var provider = CreateProvider("sqlite", connectionString: null);

        Assert.That(provider.GetConnectionString, Throws.InvalidOperationException);
    }

    [Test]
    public static void GetConnectionString_GivenWhitespaceValue_ThrowsInvalidOperationException()
    {
        var provider = CreateProvider("sqlite", "   ");

        Assert.That(provider.GetConnectionString, Throws.InvalidOperationException);
    }

    [TestCase("sqlserver", "Server=localhost", typeof(SqlServer.SqlServerConnectionFactory))]
    [TestCase("SqlServer", "Server=localhost", typeof(SqlServer.SqlServerConnectionFactory))]
    [TestCase("oracle", "Data Source=localhost", typeof(Oracle.OracleConnectionFactory))]
    [TestCase("postgresql", "Host=localhost", typeof(PostgreSql.PostgreSqlConnectionFactory))]
    [TestCase("mysql", "Server=localhost", typeof(MySql.MySqlConnectionFactory))]
    [TestCase("sqlite", "Data Source=:memory:", typeof(Sqlite.SqliteConnectionFactory))]
    public static void GetConnectionFactory_GivenSupportedDialect_ReturnsExpectedFactory(string dialect, string connectionString, Type expectedType)
    {
        var provider = CreateProvider(dialect, connectionString);

        var factory = provider.GetConnectionFactory();

        Assert.That(factory, Is.InstanceOf(expectedType));
    }

    [Test]
    public static void GetConnectionFactory_GivenMissingDialect_ThrowsInvalidOperationException()
    {
        var provider = CreateProvider(dialect: null, "Data Source=:memory:");

        Assert.That(provider.GetConnectionFactory, Throws.InvalidOperationException);
    }

    [Test]
    public static void GetConnectionFactory_GivenUnsupportedDialect_ThrowsNotSupportedException()
    {
        var provider = CreateProvider("db2", "Data Source=:memory:");

        Assert.That(provider.GetConnectionFactory, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public static void GetConnectionFactory_GivenDialectButMissingConnectionString_ThrowsInvalidOperationException()
    {
        var provider = CreateProvider("sqlite", connectionString: null);

        Assert.That(provider.GetConnectionFactory, Throws.InvalidOperationException);
    }

    [TestCase("sqlserver", "Server=localhost", typeof(SqlServer.SqlServerDialect))]
    [TestCase("oracle", "Data Source=localhost", typeof(Oracle.OracleDialect))]
    [TestCase("postgresql", "Host=localhost", typeof(PostgreSql.PostgreSqlDialect))]
    [TestCase("mysql", "Server=localhost", typeof(MySql.MySqlDialect))]
    [TestCase("sqlite", "Data Source=:memory:", typeof(Sqlite.SqliteDialect))]
    public static void GetSchematicConnection_GivenSupportedDialect_HasExpectedDialect(string dialect, string connectionString, Type expectedDialectType)
    {
        var provider = CreateProvider(dialect, connectionString);

        var connection = provider.GetSchematicConnection();

        Assert.That(connection.Dialect, Is.InstanceOf(expectedDialectType));
    }

    [Test]
    public static void GetSchematicConnection_GivenUnsupportedDialect_ThrowsNotSupportedException()
    {
        var provider = CreateProvider("db2", "Data Source=:memory:");

        Assert.That(provider.GetSchematicConnection, Throws.InstanceOf<NotSupportedException>());
    }

    [TestCase(NamingConvention.Verbatim, typeof(VerbatimNameTranslator))]
    [TestCase(NamingConvention.Pascal, typeof(PascalCaseNameTranslator))]
    [TestCase(NamingConvention.Camel, typeof(CamelCaseNameTranslator))]
    [TestCase(NamingConvention.Snake, typeof(SnakeCaseNameTranslator))]
    public static void GetNameTranslator_GivenSupportedConvention_ReturnsExpectedTranslator(NamingConvention convention, Type expectedType)
    {
        var provider = CreateProvider("sqlite", "Data Source=:memory:");

        var translator = provider.GetNameTranslator(convention);

        Assert.That(translator, Is.InstanceOf(expectedType));
    }

    [Test]
    public static void GetNameTranslator_GivenUnsupportedConvention_ThrowsNotSupportedException()
    {
        var provider = CreateProvider("sqlite", "Data Source=:memory:");

        Assert.That(
            () => provider.GetNameTranslator((NamingConvention)int.MaxValue),
            Throws.InstanceOf<NotSupportedException>());
    }
}
