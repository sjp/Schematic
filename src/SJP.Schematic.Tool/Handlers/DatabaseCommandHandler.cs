using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers
{
    internal abstract class DatabaseCommandHandler
    {
        protected DatabaseCommandHandler(FileInfo filePath)
        {
            if (!filePath.Exists)
                throw new FileNotFoundException($"Expected a configuration file at '{ filePath }', but could not find one.");

            Configuration = GetConfig(filePath.FullName);
        }

        protected IConfiguration Configuration { get; }

        protected static IConfiguration GetConfig(string filePath)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(filePath)
                .AddEnvironmentVariables()
                .Build();
        }

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

        protected IDbConnectionFactory GetConnectionFactory()
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

        protected ISchematicConnection GetSchematicConnection()
        {
            var connectionFactory = GetConnectionFactory();
            var dialect = GetDialect();

            return new SchematicConnection(connectionFactory, dialect);
        }

        protected string GetConnectionString()
        {
            var connectionString = Configuration.GetConnectionString("Schematic");
            if (connectionString.IsNullOrWhiteSpace())
                throw new InvalidOperationException(nameof(connectionString));

            return connectionString;
        }

        protected static INameTranslator GetNameTranslator(NamingConvention convention)
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
}
