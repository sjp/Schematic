using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting;
using SJP.Schematic.Reporting.Snapshot;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Tool.Handlers
{
    internal sealed class ReportCommandHandler : DatabaseCommandHandler
    {
        public ReportCommandHandler(FileInfo filePath)
            : base(filePath)
        {
            ConfigureDotPath();
        }

        public async Task<int> HandleCommandAsync(IConsole console, DirectoryInfo outputPath, CancellationToken cancellationToken)
        {
            var connection = GetSchematicConnection();
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);

            if (!outputPath.Exists)
                outputPath.Create();

            var snapshotDbPath = Path.Combine(outputPath.FullName, $"snapshot-{ Guid.NewGuid() }.db");
            var snapshotDb = await CreateSnapshotRelationalDatabaseAsync(snapshotDbPath, database).ConfigureAwait(false);

            var reportGenerator = new ReportGenerator(connection, snapshotDb, outputPath);
            await reportGenerator.GenerateAsync(cancellationToken).ConfigureAwait(false);

            if (File.Exists(snapshotDbPath))
                File.Delete(snapshotDbPath);

            console.Out.Write("Report generated to: " + outputPath.FullName);
            return ErrorCode.Success;
        }

        private static async Task<IRelationalDatabase> CreateSnapshotRelationalDatabaseAsync(string snapshotDbPath, IRelationalDatabase database)
        {
            var snapshotConnection = CreateSnapshotConnection(snapshotDbPath);
            var mapper = CreateMapper();

            var schema = new SnapshotSchema(snapshotConnection.DbConnection);
            await schema.EnsureSchemaExistsAsync().ConfigureAwait(false);

            // snapshotting
            var objectWriter = new SnapshotRelationalDatabaseWriter(snapshotConnection.DbConnection, mapper);
            await objectWriter.SnapshotDatabaseObjectsAsync(database).ConfigureAwait(false);

            // reading snapshot
            return new SnapshotRelationalDatabaseReader(snapshotConnection.DbConnection, mapper);
        }

        private void ConfigureDotPath()
        {
            var dotPath = Configuration.GetValue<string>("Graphviz:Dot");
            if (!dotPath.IsNullOrWhiteSpace())
                Environment.SetEnvironmentVariable("SCHEMATIC_GRAPHVIZ_DOT", dotPath, EnvironmentVariableTarget.Process);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(config => config.AddMaps(typeof(RelationalDatabaseTableProfile).Assembly));
            config.AssertConfigurationIsValid();
            return new Mapper(config);
        }

        private static ISchematicConnection CreateSnapshotConnection(string snapshotDbPath)
        {
            var builder = new SqliteConnectionStringBuilder { DataSource = snapshotDbPath };

            var connectionFactory = new SqliteConnectionFactory(builder.ToString());
            var dialect = new SqliteDialect();

            return new SchematicConnection(connectionFactory, dialect);
        }
    }
}
