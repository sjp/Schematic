using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess;

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

            dialect = dialect.ToLowerInvariant();
            return dialect switch
            {
                "mysql" => new MySql.MySqlConnectionFactory(),
                "oracle" => new Oracle.OracleConnectionFactory(),
                "postgresql" => new PostgreSql.PostgreSqlConnectionFactory(),
                "sqlserver" => new SqlServer.SqlServerConnectionFactory(),
                "sqlite" => new Sqlite.SqliteConnectionFactory(),
                _ => throw new NotSupportedException($"The given dialect is not supported {dialect}, expected one of: ..."),
            };
        }

        protected async Task<ISchematicConnection> GetSchematicConnectionAsync(CancellationToken cancellationToken)
        {
            var connectionFactory = GetConnectionFactory();
            var connectionString = GetConnectionString();
            var dialect = GetDialect();
            var dbConnection = await connectionFactory.CreateConnectionAsync(connectionString, cancellationToken).ConfigureAwait(false);

            return new SchematicConnection(dbConnection, dialect);
        }

        protected string GetConnectionString()
        {
            var connectionString = Configuration.GetConnectionString("Schematic");
            if (connectionString.IsNullOrWhiteSpace())
                throw new InvalidOperationException(nameof(connectionString));

            return connectionString;
        }

        protected INameTranslator GetNameTranslator(string translator)
        {
            translator = translator.ToLowerInvariant();
            return translator switch
            {
                "verbatim" => new VerbatimNameTranslator(),
                "pascal" => new PascalCaseNameTranslator(),
                "camel" => new CamelCaseNameTranslator(),
                "snake" => new SnakeCaseNameTranslator(),
                _ => throw new NotSupportedException($"The given naming convention is not supported {translator}, expected one of: ..."),
            };
        }
    }
}
