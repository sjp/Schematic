using System;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace SJP.Schematic.Tool.Handlers;

/// <summary>
/// Builds provider-specific connection strings from a set of common connection details.
/// Kept free of any interactive console dependency so it can be unit tested directly.
/// </summary>
public static class ConnectionStringFactory
{
    public readonly record struct ConnectionDetails(string Host, int? Port, string? User, string? Password, string? Database);

    public static string ForSqlite(string dataSourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataSourcePath);
        return new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = dataSourcePath }.ConnectionString;
    }

    public static string BuildGuided(string dialect, ConnectionDetails details)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dialect);

        return dialect.ToLowerInvariant() switch
        {
            "sqlserver" => BuildSqlServer(details),
            "postgresql" => BuildPostgreSql(details),
            "mysql" => BuildMySql(details),
            "oracle" => BuildOracle(details),
            "sqlite" => ForSqlite(details.Host),
            _ => throw new NotSupportedException($"The given dialect is not supported: {dialect}. Expected one of: mysql, oracle, postgresql, sqlserver, sqlite."),
        };
    }

    private static string BuildSqlServer(ConnectionDetails d)
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = d.Port.HasValue ? $"{d.Host},{d.Port.Value}" : d.Host,
        };
        if (!string.IsNullOrWhiteSpace(d.Database))
            builder.InitialCatalog = d.Database;
        if (!string.IsNullOrWhiteSpace(d.User))
            builder.UserID = d.User;
        if (!string.IsNullOrWhiteSpace(d.Password))
            builder.Password = d.Password;

        return builder.ConnectionString;
    }

    private static string BuildPostgreSql(ConnectionDetails d)
    {
        var builder = new NpgsqlConnectionStringBuilder { Host = d.Host };
        if (d.Port.HasValue)
            builder.Port = d.Port.Value;
        if (!string.IsNullOrWhiteSpace(d.Database))
            builder.Database = d.Database;
        if (!string.IsNullOrWhiteSpace(d.User))
            builder.Username = d.User;
        if (!string.IsNullOrWhiteSpace(d.Password))
            builder.Password = d.Password;

        return builder.ConnectionString;
    }

    private static string BuildMySql(ConnectionDetails d)
    {
        var builder = new MySqlConnectionStringBuilder { Server = d.Host };
        if (d.Port.HasValue)
            builder.Port = (uint)d.Port.Value;
        if (!string.IsNullOrWhiteSpace(d.Database))
            builder.Database = d.Database;
        if (!string.IsNullOrWhiteSpace(d.User))
            builder.UserID = d.User;
        if (!string.IsNullOrWhiteSpace(d.Password))
            builder.Password = d.Password;

        return builder.ConnectionString;
    }

    private static string BuildOracle(ConnectionDetails d)
    {
        // Oracle uses an "Easy Connect" data source of the form host:port/service.
        var dataSource = d.Host;
        if (d.Port.HasValue)
            dataSource += $":{d.Port.Value}";
        if (!string.IsNullOrWhiteSpace(d.Database))
            dataSource += $"/{d.Database}";

        var builder = new OracleConnectionStringBuilder { DataSource = dataSource };
        if (!string.IsNullOrWhiteSpace(d.User))
            builder.UserID = d.User;
        if (!string.IsNullOrWhiteSpace(d.Password))
            builder.Password = d.Password;

        return builder.ConnectionString;
    }
}
