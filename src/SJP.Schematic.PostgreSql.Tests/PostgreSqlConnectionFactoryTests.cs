using System;
using System.Data;
using Npgsql;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests;

internal static class PostgreSqlConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentException(string connectionString)
    {
        Assert.That(() => new PostgreSqlConnectionFactory(connectionString), Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("connectionString"));
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        using var factory = new PostgreSqlConnectionFactory("Server=127.0.0.1;");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public static void CreateConnection_GivenNoConnectionConfiguration_DoesNotThrow()
    {
        using var factory = new PostgreSqlConnectionFactory("Server=127.0.0.1;", connectionConfiguration: null);

        Assert.That(() => factory.CreateConnection(), Throws.Nothing);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_InvokesCallbackBeforeReturning()
    {
        var wasInvoked = false;
        void configureConnection(NpgsqlConnection _) => wasInvoked = true;

        using var factory = new PostgreSqlConnectionFactory(
            "Server=127.0.0.1;",
            configureConnection);

        using var connection = factory.CreateConnection();

        Assert.That(wasInvoked, Is.True);
    }

    [Test]
    public static void CreateConnection_GivenConnectionConfiguration_AppliesConfigurationToReturnedConnection()
    {
        const string expectedConnectionString = "Server=127.0.0.1;Database=other;";
        using var factory = new PostgreSqlConnectionFactory(
            "Server=127.0.0.1;",
            connection => connection.ConnectionString = expectedConnectionString);

        using var connection = factory.CreateConnection();

        Assert.That(connection.ConnectionString, Is.EqualTo(expectedConnectionString));
    }

    [Test]
    public static void MaxConcurrentQueries_WhenInvoked_ReturnsMaxPoolSizeOfDataSource()
    {
        using var factory = new DataSourceExposingConnectionFactory("Server=127.0.0.1;Maximum Pool Size=7;");

        var dataSourcePoolSize = new Npgsql.NpgsqlConnectionStringBuilder(factory.DataSourceConnectionString).MaxPoolSize;

        Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(dataSourcePoolSize));
    }

    [Test]
    public static void Ctor_GivenPoolAndTimeoutSettings_KeepsSettingsOnDataSource()
    {
        using var factory = new DataSourceExposingConnectionFactory("Host=127.0.0.1;Maximum Pool Size=37;Timeout=7;Command Timeout=45;");

        var dataSourceSettings = new Npgsql.NpgsqlConnectionStringBuilder(factory.DataSourceConnectionString);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataSourceSettings.MaxPoolSize, Is.EqualTo(37));
            Assert.That(dataSourceSettings.Timeout, Is.EqualTo(7));
            Assert.That(dataSourceSettings.CommandTimeout, Is.EqualTo(45));
            Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(37));
        }
    }

    [TestCase("Maximum Pool Size=37;")]
    [TestCase("MaxPoolSize=37;")]
    public static void MaxConcurrentQueries_GivenPoolSizeSynonym_ReturnsGivenPoolSize(string poolSizeSetting)
    {
        using var factory = new PostgreSqlConnectionFactory("Host=127.0.0.1;" + poolSizeSetting);

        Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(37));
    }

    [Test]
    public static void Ctor_GivenNoPoolOrTimeoutSettings_UsesNpgsqlDefaults()
    {
        using var factory = new DataSourceExposingConnectionFactory("Host=127.0.0.1;");

        var dataSourceSettings = new Npgsql.NpgsqlConnectionStringBuilder(factory.DataSourceConnectionString);
        var npgsqlDefaults = new Npgsql.NpgsqlConnectionStringBuilder();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataSourceSettings.MaxPoolSize, Is.EqualTo(npgsqlDefaults.MaxPoolSize));
            Assert.That(dataSourceSettings.Timeout, Is.EqualTo(npgsqlDefaults.Timeout));
            Assert.That(dataSourceSettings.CommandTimeout, Is.EqualTo(npgsqlDefaults.CommandTimeout));
            Assert.That(factory.MaxConcurrentQueries, Is.EqualTo(npgsqlDefaults.MaxPoolSize));
        }
    }

    private sealed class DataSourceExposingConnectionFactory(string connectionString) : PostgreSqlConnectionFactory(connectionString)
    {
        public string DataSourceConnectionString => DataSource.ConnectionString;
    }
}