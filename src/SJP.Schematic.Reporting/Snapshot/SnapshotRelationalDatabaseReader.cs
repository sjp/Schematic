using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Snapshot.Query;
using SJP.Schematic.Reporting.Snapshot.QueryResult;

namespace SJP.Schematic.Reporting.Snapshot
{
    public class SnapshotRelationalDatabaseReader : IRelationalDatabase
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

        public SnapshotRelationalDatabaseReader(IDbConnectionFactory connection, IMapper mapper)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private IDbConnectionFactory Connection { get; }

        private IMapper Mapper { get; }

        public IIdentifierDefaults IdentifierDefaults => GetIdentifierDefaults();

        private async Task<IIdentifierDefaults> GetIdentifierDefaultsAsync(CancellationToken cancellationToken)
        {
            return await Connection.QueryFirstOrNone<GetIdentifierDefaultsQueryResult>(
                DatabaseIdentifierDefaultsQuery,
                cancellationToken
            ).MatchUnsafe(
                r => new IdentifierDefaults(r.Server, r.Database, r.Schema),
                () => new IdentifierDefaults(null, null, null)
            ).ConfigureAwait(false);
        }

        private IIdentifierDefaults GetIdentifierDefaults()
        {
            using var connection = Connection.CreateConnection();
            var dbRecord = connection.QueryFirstOrDefault<GetIdentifierDefaultsQueryResult>(DatabaseIdentifierDefaultsQuery);
            return dbRecord != null
                ? new IdentifierDefaults(dbRecord.Server, dbRecord.Database, dbRecord.Schema)
                : new IdentifierDefaults(null, null, null);
        }

        public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var routineNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Routine },
                cancellationToken
            ).ConfigureAwait(false);

            var routines = routineNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetRoutine(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var routine in routines.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var routineOption = await routine.ToOption().ConfigureAwait(false);
                if (routineOption.IsNone)
                    continue;

                yield return routineOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IDatabaseSequence> GetAllSequences([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var sequenceNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Sequence },
                cancellationToken
            ).ConfigureAwait(false);

            var sequences = sequenceNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetSequence(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var sequence in sequences.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var sequenceOption = await sequence.ToOption().ConfigureAwait(false);
                if (sequenceOption.IsNone)
                    continue;

                yield return sequenceOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var synonymNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Synonym },
                cancellationToken
            ).ConfigureAwait(false);

            var synonyms = synonymNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetSynonym(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var synonym in synonyms.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var synonymOption = await synonym.ToOption().ConfigureAwait(false);
                if (synonymOption.IsNone)
                    continue;

                yield return synonymOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tableNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Table },
                cancellationToken
            ).ConfigureAwait(false);

            var tables = tableNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetTable(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var table in tables.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var tableOption = await table.ToOption().ConfigureAwait(false);
                if (tableOption.IsNone)
                    continue;

                yield return tableOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IDatabaseView> GetAllViews([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var viewNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.View },
                cancellationToken
            ).ConfigureAwait(false);

            var views = viewNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetView(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var view in views.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var viewOption = await view.ToOption().ConfigureAwait(false);
                if (viewOption.IsNone)
                    continue;

                yield return viewOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(routineName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseObjectDefinitionQueryResult>(
                       DatabaseObjectDefinitionQuery,
                       new GetDatabaseObjectDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Routine,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.DefinitionJson != null)
                .Map<IDatabaseRoutine>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.DatabaseRoutine>(r.DefinitionJson!, _settings.Value);
                    return Mapper.Map<DatabaseRoutine>(dto);
                });
        }

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(sequenceName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseObjectDefinitionQueryResult>(
                       DatabaseObjectDefinitionQuery,
                       new GetDatabaseObjectDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Sequence,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.DefinitionJson != null)
                .Map<IDatabaseSequence>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.DatabaseSequence>(r.DefinitionJson!, _settings.Value);
                    return Mapper.Map<DatabaseSequence>(dto);
                });
        }

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(synonymName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseObjectDefinitionQueryResult>(
                       DatabaseObjectDefinitionQuery,
                       new GetDatabaseObjectDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Synonym,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.DefinitionJson != null)
                .Map<IDatabaseSynonym>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.DatabaseSynonym>(r.DefinitionJson!, _settings.Value);
                    return Mapper.Map<DatabaseSynonym>(dto);
                });
        }

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(tableName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseObjectDefinitionQueryResult>(
                       DatabaseObjectDefinitionQuery,
                       new GetDatabaseObjectDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Table,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.DefinitionJson != null)
                .Map<IRelationalDatabaseTable>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.RelationalDatabaseTable>(r.DefinitionJson!, _settings.Value);
                    return Mapper.Map<RelationalDatabaseTable>(dto);
                });
        }

        public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(viewName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseObjectDefinitionQueryResult>(
                       DatabaseObjectDefinitionQuery,
                       new GetDatabaseObjectDefinitionQuery
                       {
                           ObjectType = ObjectTypes.View,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.DefinitionJson != null)
                .Map<IDatabaseView>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.DatabaseView>(r.DefinitionJson!, _settings.Value);
                    return Mapper.Map<DatabaseView>(dto);
                });
        }

        private Task<Identifier> QualifyObjectNameAsync(Identifier identifier, CancellationToken cancellationToken)
        {
            return GetIdentifierDefaultsAsync(cancellationToken)
                .Map(n => Identifier.CreateQualifiedIdentifier(
                    identifier.Server ?? n.Server,
                    identifier.Database ?? n.Database,
                    identifier.Schema ?? n.Schema,
                    identifier.LocalName
                ));
        }

        private static readonly string AllObjectNamesForTypeQuery = @$"
SELECT
    server_name AS ""{ nameof(GetAllObjectNamesForTypeQueryResult.ServerName) }"",
    database_name AS ""{ nameof(GetAllObjectNamesForTypeQueryResult.DatabaseName) }"",
    schema_name AS ""{ nameof(GetAllObjectNamesForTypeQueryResult.SchemaName) }"",
    local_name AS ""{ nameof(GetAllObjectNamesForTypeQueryResult.LocalName) }""
FROM database_object
WHERE
    object_type = @{ nameof(GetAllObjectNamesForTypeQuery.ObjectType) }
";

        private static readonly string DatabaseIdentifierDefaultsQuery = @$"
SELECT
    server_name AS ""{ nameof(GetIdentifierDefaultsQueryResult.Server) }"",
    database_name AS ""{ nameof(GetIdentifierDefaultsQueryResult.Database) }"",
    schema_name AS ""{ nameof(GetIdentifierDefaultsQueryResult.Schema) }""
FROM database_identifier_defaults
ORDER BY ROWID ASC LIMIT 1";

        private static readonly string DatabaseObjectDefinitionQuery = @$"
SELECT
    server_name AS ""{ nameof(GetDatabaseObjectDefinitionQueryResult.ServerName) }"",
    database_name AS ""{ nameof(GetDatabaseObjectDefinitionQueryResult.DatabaseName) }"",
    schema_name AS ""{ nameof(GetDatabaseObjectDefinitionQueryResult.SchemaName) }"",
    local_name AS ""{ nameof(GetDatabaseObjectDefinitionQueryResult.LocalName) }"",
    definition_json AS ""{ nameof(GetDatabaseObjectDefinitionQueryResult.DefinitionJson) }""
FROM database_object
WHERE
    object_type = @{ nameof(GetDatabaseObjectDefinitionQuery.ObjectType) }
    AND local_name = @{ nameof(GetDatabaseObjectDefinitionQuery.LocalName) }
    AND schema_name = @{ nameof(GetDatabaseObjectDefinitionQuery.SchemaName) }
    AND ((database_name IS NULL AND @{ nameof(GetDatabaseObjectDefinitionQuery.DatabaseName) } IS NULL) OR (database_name = @{ nameof(GetDatabaseObjectDefinitionQuery.DatabaseName) }))";
    }
}
