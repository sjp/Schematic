using System;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers;

public class DatabaseCommandDependencyProvider : IDatabaseCommandDependencyProvider
{
    public DatabaseCommandDependencyProvider(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    private IDatabaseDialect GetDialect()
    {
        var dialect = Configuration.GetValue<string>("Dialect");
        if (dialect.IsNullOrWhiteSpace())
            throw new InvalidOperationException(nameof(dialect));

        dialect = dialect.ToLowerInvariant();
        return dialect switch
        {
            "mysql" => new MySql.MySqlDialect(),
            "oracle" => new Oracle.OracleDialect(),
            "postgresql" => new PostgreSql.PostgreSqlDialect(),
            "sqlserver" => new SqlServer.SqlServerDialect(),
            "sqlite" => new Sqlite.SqliteDialect(),
            _ => throw new NotSupportedException($"The given dialect is not supported {dialect}, expected one of: ..."),
        };
    }

    public IDbConnectionFactory GetConnectionFactory()
    {
        var dialect = Configuration.GetValue<string>("Dialect");
        if (dialect.IsNullOrWhiteSpace())
            throw new InvalidOperationException(nameof(dialect));

        var connectionString = GetConnectionString();
        dialect = dialect.ToLowerInvariant();
        return dialect switch
        {
            "mysql" => new MySql.MySqlConnectionFactory(connectionString),
            "oracle" => new Oracle.OracleConnectionFactory(connectionString),
            "postgresql" => new PostgreSql.PostgreSqlConnectionFactory(connectionString),
            "sqlserver" => new SqlServer.SqlServerConnectionFactory(connectionString),
            "sqlite" => new Sqlite.SqliteConnectionFactory(connectionString),
            _ => throw new NotSupportedException($"The given dialect is not supported {dialect}, expected one of: ..."),
        };
    }

    public ISchematicConnection GetSchematicConnection()
    {
        var connectionFactory = GetConnectionFactory();
        var dialect = GetDialect();

        return new SchematicConnection(connectionFactory, dialect);
    }

    public IRelationalDatabaseProvider GetRelationalDatabaseProvider(ISchematicConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var dialect = Configuration.GetValue<string>("Dialect");
        if (dialect.IsNullOrWhiteSpace())
            throw new InvalidOperationException(nameof(dialect));

        dialect = dialect.ToLowerInvariant();
        return dialect switch
        {
            "mysql" => new MySql.MySqlDatabaseProvider(connection),
            "oracle" => new Oracle.OracleDatabaseProvider(connection),
            "postgresql" => new PostgreSql.PostgreSqlDatabaseProvider(connection),
            "sqlserver" => new SqlServer.SqlServerDatabaseProvider(connection),
            "sqlite" => new Sqlite.SqliteDatabaseProvider(connection),
            _ => throw new NotSupportedException($"The given dialect is not supported {dialect}, expected one of: ..."),
        };
    }

    public string GetConnectionString()
    {
        var connectionString = Configuration.GetConnectionString("Schematic");
        if (connectionString.IsNullOrWhiteSpace())
            throw new InvalidOperationException(nameof(connectionString));

        return connectionString;
    }

    public INameTranslator GetNameTranslator(NamingConvention convention)
    {
        return convention switch
        {
            NamingConvention.Verbatim => new VerbatimNameTranslator(),
            NamingConvention.Pascal => new PascalCaseNameTranslator(),
            NamingConvention.Camel => new CamelCaseNameTranslator(),
            NamingConvention.Snake => new SnakeCaseNameTranslator(),
            _ => throw new NotSupportedException($"The given naming convention is not supported {convention}, expected one of: ..."),
        };
    }
}
