using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Configuration;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql;
using SJP.Schematic.PostgreSql;
using SJP.Schematic.Sqlite;
using SJP.Schematic.SqlServer;
using System.Data;

namespace SJP.Schematic.Tool
{
    internal abstract class DatabaseCommand
    {
        protected DatabaseCommand(CommandLineApplication application)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
        }

        private CommandLineApplication Application { get; }

        [Option(Description = "A connection string", LongName = "connection-string", ShortName = "cs")]
        public string ConnectionString { get; set; }

        [Required]
        [AllowedValues("sqlite", "sqlserver", "postgresql", "mysql", Comparer = StringComparison.OrdinalIgnoreCase)]
        [Option(Description = "The database dialect", LongName = "database-dialect", ShortName = "dd")]
        public string DatabaseDialect { get; set; }

        [Option(Description = "A JSON configuration file containing a connection", LongName = "connection-config-json", ShortName = "csj")]
        [LegalFilePath]
        [FileExists]
        public string ConnectionConfigJson { get; set; }

        [Option(Description = "An XML configuration file containing a connection", LongName = "connection-config-xml", ShortName = "csx")]
        [LegalFilePath]
        [FileExists]
        public string ConnectionConfigXml { get; set; }

        [Option(Description = "The name of the connection string in a configuration file", LongName = "connection-config-name", ShortName = "csname")]
        public string ConnectionConfigName { get; set; }

        public static ConnectionStatus GetConnectionStatus(IDatabaseDialect dialect, string connectionString)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));

            try
            {
                var connection = dialect.CreateConnection(connectionString);
                return ConnectionStatus.Success(connection);
            }
            catch (Exception ex)
            {
                return ConnectionStatus.Failure(ex);
            }
        }

        public bool TryGetConnectionString(out string connectionString)
        {
            if (!ConnectionString.IsNullOrWhiteSpace())
            {
                connectionString = ConnectionString;
                return true;
            }

            if (!ConnectionConfigName.IsNullOrWhiteSpace())
            {
                IConfigurationRoot configuration = null;
                string configurationFile = null;

                if (!ConnectionConfigJson.IsNullOrWhiteSpace())
                {
                    var builder = new ConfigurationBuilder().AddJsonFile(ConnectionConfigJson);
                    configuration = builder.Build();
                    configurationFile = ConnectionConfigJson;
                }
                else if (!ConnectionConfigXml.IsNullOrWhiteSpace())
                {
                    var builder = new ConfigurationBuilder().AddXmlFile(ConnectionConfigXml);
                    configuration = builder.Build();
                    configurationFile = ConnectionConfigXml;
                }
                else
                {
                    Application.Error.WriteLine("A connection configuration name was given but no corresponding configuration file was provided.");
                    connectionString = null;
                    return false;
                }

                var connectionStringSection = configuration.GetSection("ConnectionStrings");
                var connections = connectionStringSection.AsEnumerable();
                var matchingConnection = connections.FirstOrDefault(c => c.Key == ConnectionConfigName);
                if (matchingConnection.Key != ConnectionConfigName)
                {
                    Application.Error.WriteLine($"Could not find a connection named '{ ConnectionConfigName }' in '{ configurationFile }'.");
                    connectionString = null;
                    return false;
                }

                connectionString = matchingConnection.Value;
                return true;
            }

            Application.Error.WriteLine("No valid connection string or connection configuration provided.");

            connectionString = null;
            return false;
        }

        public IDatabaseDialect GetDatabaseDialect()
        {
            if (!_dialects.TryGetValue(DatabaseDialect, out var dialect))
                throw new NotSupportedException("Unsupported dialect: " + DatabaseDialect);

            return dialect;
        }

        public Func<IDatabaseDialect, IDbConnection, IIdentifierDefaults, IRelationalDatabase> GetRelationalDatabaseFactory()
        {
            if (!_databaseFactories.TryGetValue(DatabaseDialect, out var factory))
                throw new NotSupportedException("Unsupported dialect: " + DatabaseDialect);

            return factory;
        }

        private readonly static IReadOnlyDictionary<string, IDatabaseDialect> _dialects = new Dictionary<string, IDatabaseDialect>
        {
            ["sqlite"] = new SqliteDialect(),
            ["sqlserver"] = new SqlServerDialect(),
            ["mysql"] = new MySqlDialect(),
            ["postgresql"] = new PostgreSqlDialect()
        };

        private readonly static IReadOnlyDictionary<string, Func<IDatabaseDialect, IDbConnection, IIdentifierDefaults, IRelationalDatabase>> _databaseFactories =
            new Dictionary<string, Func<IDatabaseDialect, IDbConnection, IIdentifierDefaults, IRelationalDatabase>>
            {
                ["sqlite"] = (d, c, i) => new SqliteRelationalDatabase(d, c, i),
                ["sqlserver"] = (d, c, i) => new SqlServerRelationalDatabase(d, c, i),
                ["mysql"] = (d, c, i) => new MySqlRelationalDatabase(d, c, i),
                ["postgresql"] = (d, c, i) => new PostgreSqlRelationalDatabase(d, c, i, new DefaultPostgreSqlIdentifierResolutionStrategy())
            };

        public sealed class ConnectionStatus
        {
            private ConnectionStatus(bool isConnected, IDbConnection connection, Exception exception)
            {
                IsConnected = isConnected;
                Connection = connection;
                Error = exception;
            }

            public static ConnectionStatus Success(IDbConnection connection)
            {
                if (connection == null)
                    throw new ArgumentNullException(nameof(connection));

                return new ConnectionStatus(true, connection, null);
            }

            public static ConnectionStatus Failure(Exception exception)
            {
                if (exception == null)
                    throw new ArgumentNullException(nameof(exception));

                return new ConnectionStatus(false, null, exception);
            }

            public bool IsConnected { get; }

            public IDbConnection Connection { get; }

            public Exception Error { get; }
        }
    }
}
