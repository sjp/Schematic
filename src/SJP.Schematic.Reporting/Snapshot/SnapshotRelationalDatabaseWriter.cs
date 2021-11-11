using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Snapshot.Query;

namespace SJP.Schematic.Reporting.Snapshot
{
    public class SnapshotRelationalDatabaseWriter
    {
        private static readonly Lazy<JsonSerializerOptions> _settings = new(LoadSettings);

        private static JsonSerializerOptions LoadSettings()
        {
            var settings = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            settings.Converters.Add(new JsonStringEnumConverter());

            return settings;
        }

        public SnapshotRelationalDatabaseWriter(IDbConnectionFactory connection, IMapper mapper)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private IDbConnectionFactory Connection { get; }

        private IMapper Mapper { get; }

        public Task SnapshotDatabaseObjectsAsync(IRelationalDatabase database, CancellationToken cancellationToken = default)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return SnapshotDatabaseObjectsCoreAsync(database, cancellationToken);
        }

        private async Task SnapshotDatabaseObjectsCoreAsync(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            await AddDatabaseIdentifierDefaultsAsync(database.IdentifierDefaults, cancellationToken).ConfigureAwait(false);

            await foreach (var table in database.GetAllTables(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddTableAsync(table, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var view in database.GetAllViews(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddViewAsync(view, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var sequence in database.GetAllSequences(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddSequenceAsync(sequence, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var synonym in database.GetAllSynonyms(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddSynonymAsync(synonym, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var routine in database.GetAllRoutines(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddRoutineAsync(routine, cancellationToken).ConfigureAwait(false);
            }
        }

        private Task AddDatabaseIdentifierDefaultsAsync(IIdentifierDefaults identifierDefaults, CancellationToken cancellationToken)
        {
            if (identifierDefaults == null)
                throw new ArgumentNullException(nameof(identifierDefaults));

            return Connection.ExecuteAsync(
                InsertDatabaseDefaultsQuery,
                new AddDatabaseDefaultsQuery
                {
                    ServerName = identifierDefaults.Server,
                    DatabaseName = identifierDefaults.Database,
                    SchemaName = identifierDefaults.Schema
                },
                cancellationToken
            );
        }

        private Task AddTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var dto = Mapper.Map<IRelationalDatabaseTable, Serialization.Dto.RelationalDatabaseTable>(table);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseObjectQuery,
                new AddDatabaseObjectQuery
                {
                    ObjectType = ObjectTypes.Table,
                    ServerName = table.Name.Server,
                    DatabaseName = table.Name.Database,
                    SchemaName = table.Name.Schema,
                    LocalName = table.Name.LocalName,
                    DefinitionJson = json
                },
                cancellationToken
            );
        }

        private Task AddViewAsync(IDatabaseView view, CancellationToken cancellationToken)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var dto = Mapper.Map<IDatabaseView, Serialization.Dto.DatabaseView>(view);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseObjectQuery,
                new AddDatabaseObjectQuery
                {
                    ObjectType = ObjectTypes.View,
                    ServerName = view.Name.Server,
                    DatabaseName = view.Name.Database,
                    SchemaName = view.Name.Schema,
                    LocalName = view.Name.LocalName,
                    DefinitionJson = json
                },
                cancellationToken
            );
        }

        private Task AddSequenceAsync(IDatabaseSequence sequence, CancellationToken cancellationToken)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var dto = Mapper.Map<IDatabaseSequence, Serialization.Dto.DatabaseSequence>(sequence);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseObjectQuery,
                new AddDatabaseObjectQuery
                {
                    ObjectType = ObjectTypes.Sequence,
                    ServerName = sequence.Name.Server,
                    DatabaseName = sequence.Name.Database,
                    SchemaName = sequence.Name.Schema,
                    LocalName = sequence.Name.LocalName,
                    DefinitionJson = json
                },
                cancellationToken
            );
        }

        private Task AddSynonymAsync(IDatabaseSynonym synonym, CancellationToken cancellationToken)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            var dto = Mapper.Map<IDatabaseSynonym, Serialization.Dto.DatabaseSynonym>(synonym);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseObjectQuery,
                new AddDatabaseObjectQuery
                {
                    ObjectType = ObjectTypes.Synonym,
                    ServerName = synonym.Name.Server,
                    DatabaseName = synonym.Name.Database,
                    SchemaName = synonym.Name.Schema,
                    LocalName = synonym.Name.LocalName,
                    DefinitionJson = json
                },
                cancellationToken
            );
        }

        private Task AddRoutineAsync(IDatabaseRoutine routine, CancellationToken cancellationToken)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            var dto = Mapper.Map<IDatabaseRoutine, Serialization.Dto.DatabaseRoutine>(routine);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseObjectQuery,
                new AddDatabaseObjectQuery
                {
                    ObjectType = ObjectTypes.Routine,
                    ServerName = routine.Name.Server,
                    DatabaseName = routine.Name.Database,
                    SchemaName = routine.Name.Schema,
                    LocalName = routine.Name.LocalName,
                    DefinitionJson = json
                },
                cancellationToken
            );
        }

        private static readonly string InsertDatabaseDefaultsQuery = @$"
INSERT INTO database_identifier_defaults (
    server_name,
    database_name,
    schema_name
)
VALUES (
    @{ nameof(AddDatabaseDefaultsQuery.ServerName) },
    @{ nameof(AddDatabaseDefaultsQuery.DatabaseName) },
    @{ nameof(AddDatabaseDefaultsQuery.SchemaName) }
)";

        private static readonly string InsertDatabaseObjectQuery = @$"
INSERT INTO database_object (
    object_type,
    server_name,
    database_name,
    schema_name,
    local_name,
    definition_json
)
VALUES (
    @{ nameof(AddDatabaseObjectQuery.ObjectType) },
    @{ nameof(AddDatabaseObjectQuery.ServerName) },
    @{ nameof(AddDatabaseObjectQuery.DatabaseName) },
    @{ nameof(AddDatabaseObjectQuery.SchemaName) },
    @{ nameof(AddDatabaseObjectQuery.LocalName) },
    @{ nameof(AddDatabaseObjectQuery.DefinitionJson) }
)";
    }
}
