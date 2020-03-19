using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql;
using SJP.Schematic.PostgreSql;
using SJP.Schematic.Sqlite;
using SJP.Schematic.SqlServer;
using SJP.Schematic.Oracle;

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
        [AllowedValues("sqlite", "sqlserver", "postgresql", "mysql", "oracle", Comparer = StringComparison.OrdinalIgnoreCase)]
        [Option(Description = "The database dialect", LongName = "dialect", ShortName = "d")]
        public string DatabaseDialect { get; set; }

        [Option(Description = "A JSON configuration file containing a connection", LongName = "config", ShortName = "c")]
        [LegalFilePath]
        [FileExists]
        public string ConnectionConfigJson { get; set; }

        [Option(Description = "The name of the connection string in a configuration file", LongName = "config-name", ShortName = "name")]
        public string ConnectionConfigName { get; set; }

        public Task<ConnectionStatus> GetConnectionStatusAsync(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(connectionString));
            if (!_connectionFactories.ContainsKey(DatabaseDialect))
                throw new NotSupportedException("Unsupported dialect: " + DatabaseDialect);

            return GetConnectionStatusAsyncCore(connectionString);
        }

        private async Task<ConnectionStatus> GetConnectionStatusAsyncCore(string connectionString)
        {
            try
            {
                var factory = _connectionFactories[DatabaseDialect];
                var connection = await factory.Invoke(connectionString).ConfigureAwait(false);
                connection.Dispose();
                return ConnectionStatus.Success(connection);
            }
            catch (Exception ex)
            {
                return ConnectionStatus.Failure(ex);
            }
        }

        public async Task<string> TryGetConnectionStringAsync()
        {
            if (!ConnectionString.IsNullOrWhiteSpace())
                return ConnectionString;

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
                else
                {
                    await Application.Error.WriteLineAsync("A connection configuration name was given but no corresponding configuration file was provided.").ConfigureAwait(false);
                    return null;
                }

                var connectionStringSection = configuration.GetSection("ConnectionStrings");
                var connections = connectionStringSection.AsEnumerable();
                var matchingConnection = connections.FirstOrDefault(c => c.Key == ConnectionConfigName);
                if (matchingConnection.Key != ConnectionConfigName)
                {
                    await Application.Error.WriteLineAsync($"Could not find a connection named '{ ConnectionConfigName }' in '{ configurationFile }'.").ConfigureAwait(false);
                    return null;
                }

                return matchingConnection.Value;
            }

            await Application.Error.WriteLineAsync("No valid connection string or connection configuration provided.").ConfigureAwait(false);

            return null;
        }

        public IDatabaseDialect GetDatabaseDialect()
        {
            if (!_dialectFactories.TryGetValue(DatabaseDialect, out var dialect))
                throw new NotSupportedException("Unsupported dialect: " + DatabaseDialect);

            return dialect.Invoke();
        }

        private static readonly IReadOnlyDictionary<string, Func<string, Task<IDbConnection>>> _connectionFactories = new Dictionary<string, Func<string, Task<IDbConnection>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["sqlite"] = cs => SqliteDialect.CreateConnectionAsync(cs),
            ["sqlserver"] = cs => SqlServerDialect.CreateConnectionAsync(cs),
            ["mysql"] = cs => MySqlDialect.CreateConnectionAsync(cs),
            ["oracle"] = cs => OracleDialect.CreateConnectionAsync(cs),
            ["postgresql"] = cs => PostgreSqlDialect.CreateConnectionAsync(cs)
        };

        private static readonly IReadOnlyDictionary<string, Func<IDatabaseDialect>> _dialectFactories = new Dictionary<string, Func<IDatabaseDialect>>(StringComparer.OrdinalIgnoreCase)
        {
            ["sqlite"] = () => new SqliteDialect(),
            ["sqlserver"] = () => new SqlServerDialect(),
            ["mysql"] = () => new MySqlDialect(),
            ["oracle"] = () => new OracleDialect(),
            ["postgresql"] = () => new PostgreSqlDialect()
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
