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
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Snapshot.Query;
using SJP.Schematic.Reporting.Snapshot.QueryResult;

namespace SJP.Schematic.Reporting.Snapshot
{
    public class SnapshotRelationalDatabaseCommentsReader : IRelationalDatabaseCommentProvider
    {
        private static readonly Lazy<JsonSerializerOptions> _settings = new(LoadSettings);

        private static JsonSerializerOptions LoadSettings()
        {
            var settings = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
            };
            settings.Converters.Add(new JsonStringEnumConverter());

            return settings;
        }

        public SnapshotRelationalDatabaseCommentsReader(IDbConnectionFactory connection, IMapper mapper)
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

        public async IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var routineNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Routine },
                cancellationToken
            ).ConfigureAwait(false);

            var routineComments = routineNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetRoutineComments(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var routineComment in routineComments.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var commentOption = await routineComment.ToOption().ConfigureAwait(false);
                if (commentOption.IsNone)
                    continue;

                yield return commentOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var sequenceNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Sequence },
                cancellationToken
            ).ConfigureAwait(false);

            var sequenceComments = sequenceNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetSequenceComments(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var sequenceComment in sequenceComments.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var commentOption = await sequenceComment.ToOption().ConfigureAwait(false);
                if (commentOption.IsNone)
                    continue;

                yield return commentOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var synonymNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Synonym },
                cancellationToken
            ).ConfigureAwait(false);

            var synonymComments = synonymNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetSynonymComments(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var synonymComment in synonymComments.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var commentOption = await synonymComment.ToOption().ConfigureAwait(false);
                if (commentOption.IsNone)
                    continue;

                yield return commentOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tableNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.Table },
                cancellationToken
            ).ConfigureAwait(false);

            var tableComments = tableNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetTableComments(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var table in tableComments.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var commentOption = await table.ToOption().ConfigureAwait(false);
                if (commentOption.IsNone)
                    continue;

                yield return commentOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public async IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var viewNames = await Connection.QueryAsync<GetAllObjectNamesForTypeQueryResult>(
                AllObjectNamesForTypeQuery,
                new GetAllObjectNamesForTypeQuery { ObjectType = ObjectTypes.View },
                cancellationToken
            ).ConfigureAwait(false);

            var viewComments = viewNames
                .Select(n => Identifier.CreateQualifiedIdentifier(
                    n.ServerName,
                    n.DatabaseName,
                    n.SchemaName,
                    n.LocalName
                ))
                .Select(n => GetViewComments(n, cancellationToken))
                .ToAsyncEnumerable();

            await foreach (var viewComment in viewComments.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var commentOption = await viewComment.ToOption().ConfigureAwait(false);
                if (commentOption.IsNone)
                    continue;

                yield return commentOption.MatchUnsafe(t => t, () => default!);
            }
        }

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(routineName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseCommentDefinitionQueryResult>(
                       DatabaseCommentDefinitionQuery,
                       new GetDatabaseCommentDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Routine,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.CommentJson != null)
                .Map<IDatabaseRoutineComments>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.Comments.DatabaseRoutineComments>(r.CommentJson!, _settings.Value);
                    return Mapper.Map<DatabaseRoutineComments>(dto);
                });
        }

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(sequenceName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseCommentDefinitionQueryResult>(
                       DatabaseCommentDefinitionQuery,
                       new GetDatabaseCommentDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Sequence,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.CommentJson != null)
                .Map<IDatabaseSequenceComments>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.Comments.DatabaseSequenceComments>(r.CommentJson!, _settings.Value);
                    return Mapper.Map<DatabaseSequenceComments>(dto);
                });
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(synonymName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseCommentDefinitionQueryResult>(
                       DatabaseCommentDefinitionQuery,
                       new GetDatabaseCommentDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Synonym,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.CommentJson != null)
                .Map<IDatabaseSynonymComments>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.Comments.DatabaseSynonymComments>(r.CommentJson!, _settings.Value);
                    return Mapper.Map<DatabaseSynonymComments>(dto);
                });
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(tableName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseCommentDefinitionQueryResult>(
                       DatabaseCommentDefinitionQuery,
                       new GetDatabaseCommentDefinitionQuery
                       {
                           ObjectType = ObjectTypes.Table,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.CommentJson != null)
                .Map<IRelationalDatabaseTableComments>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.Comments.DatabaseTableComments>(r.CommentJson!, _settings.Value);
                    return Mapper.Map<RelationalDatabaseTableComments>(dto);
                });
        }

        public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var dbRecord = OptionAsync<Identifier>.OptionalAsync(QualifyObjectNameAsync(viewName, cancellationToken))
                .Bind(n =>
                   Connection.QuerySingleOrNone<GetDatabaseCommentDefinitionQueryResult>(
                       DatabaseCommentDefinitionQuery,
                       new GetDatabaseCommentDefinitionQuery
                       {
                           ObjectType = ObjectTypes.View,
                           DatabaseName = n.Database,
                           SchemaName = n.Schema,
                           LocalName = n.LocalName
                       },
                       cancellationToken
                   ));

            return dbRecord
                .Where(r => r.CommentJson != null)
                .Map<IDatabaseViewComments>(r =>
                {
                    var dto = JsonSerializer.Deserialize<Serialization.Dto.Comments.DatabaseViewComments>(r.CommentJson!, _settings.Value);
                    return Mapper.Map<DatabaseViewComments>(dto);
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
FROM database_comment
WHERE
    object_type = @{ nameof(GetAllObjectNamesForTypeQuery.ObjectType) }";

        private static readonly string DatabaseIdentifierDefaultsQuery = @$"
SELECT
    server_name AS ""{ nameof(GetIdentifierDefaultsQueryResult.Server) }"",
    database_name AS ""{ nameof(GetIdentifierDefaultsQueryResult.Database) }"",
    schema_name AS ""{ nameof(GetIdentifierDefaultsQueryResult.Schema) }""
FROM database_identifier_defaults
ORDER BY ROWID ASC LIMIT 1";

        private static readonly string DatabaseCommentDefinitionQuery = @$"
SELECT
    server_name AS ""{ nameof(GetDatabaseCommentDefinitionQueryResult.ServerName) }"",
    database_name AS ""{ nameof(GetDatabaseCommentDefinitionQueryResult.DatabaseName) }"",
    schema_name AS ""{ nameof(GetDatabaseCommentDefinitionQueryResult.SchemaName) }"",
    local_name AS ""{ nameof(GetDatabaseCommentDefinitionQueryResult.LocalName) }"",
    comment_json AS ""{ nameof(GetDatabaseCommentDefinitionQueryResult.CommentJson) }""
FROM database_comment
WHERE
    object_type = @{ nameof(GetDatabaseCommentDefinitionQuery.ObjectType) }
    AND local_name = @{ nameof(GetDatabaseCommentDefinitionQuery.LocalName) }
    AND schema_name = @{ nameof(GetDatabaseCommentDefinitionQuery.SchemaName) }
    AND ((database_name IS NULL AND @{ nameof(GetDatabaseCommentDefinitionQuery.DatabaseName) } IS NULL) OR (database_name = @{ nameof(GetDatabaseCommentDefinitionQuery.DatabaseName) }))";
    }
}
