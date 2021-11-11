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
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Snapshot.Query;

namespace SJP.Schematic.Reporting.Snapshot
{
    public class SnapshotRelationalDatabaseCommentsWriter
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

        public SnapshotRelationalDatabaseCommentsWriter(IDbConnectionFactory connection, IMapper mapper)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        private IDbConnectionFactory Connection { get; }

        private IMapper Mapper { get; }

        public Task SnapshotDatabaseCommentsAsync(IRelationalDatabaseCommentProvider commentProvider, CancellationToken cancellationToken = default)
        {
            if (commentProvider == null)
                throw new ArgumentNullException(nameof(commentProvider));

            return SnapshotDatabaseCommentsCoreAsync(commentProvider, cancellationToken);
        }

        private async Task SnapshotDatabaseCommentsCoreAsync(IRelationalDatabaseCommentProvider commentProvider, CancellationToken cancellationToken)
        {
            await AddDatabaseIdentifierDefaultsAsync(commentProvider.IdentifierDefaults, cancellationToken).ConfigureAwait(false);

            await foreach (var tableComments in commentProvider.GetAllTableComments(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddTableCommentsAsync(tableComments, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var viewComments in commentProvider.GetAllViewComments(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddViewCommentsAsync(viewComments, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var sequenceComments in commentProvider.GetAllSequenceComments(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddSequenceCommentsAsync(sequenceComments, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var synonymComments in commentProvider.GetAllSynonymComments(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddSynonymCommentsAsync(synonymComments, cancellationToken).ConfigureAwait(false);
            }

            await foreach (var routineComments in commentProvider.GetAllRoutineComments(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await AddRoutineCommentsAsync(routineComments, cancellationToken).ConfigureAwait(false);
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

        private Task AddTableCommentsAsync(IRelationalDatabaseTableComments tableComments, CancellationToken cancellationToken)
        {
            if (tableComments == null)
                throw new ArgumentNullException(nameof(tableComments));

            var dto = Mapper.Map<IRelationalDatabaseTableComments, Serialization.Dto.Comments.DatabaseTableComments>(tableComments);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseCommentsQuery,
                new AddDatabaseCommentQuery
                {
                    ObjectType = ObjectTypes.Table,
                    ServerName = tableComments.TableName.Server,
                    DatabaseName = tableComments.TableName.Database,
                    SchemaName = tableComments.TableName.Schema,
                    LocalName = tableComments.TableName.LocalName,
                    CommentJson = json
                },
                cancellationToken
            );
        }

        private Task AddViewCommentsAsync(IDatabaseViewComments viewComments, CancellationToken cancellationToken)
        {
            if (viewComments == null)
                throw new ArgumentNullException(nameof(viewComments));

            var dto = Mapper.Map<IDatabaseViewComments, Serialization.Dto.Comments.DatabaseViewComments>(viewComments);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseCommentsQuery,
                new AddDatabaseCommentQuery
                {
                    ObjectType = ObjectTypes.View,
                    ServerName = viewComments.ViewName.Server,
                    DatabaseName = viewComments.ViewName.Database,
                    SchemaName = viewComments.ViewName.Schema,
                    LocalName = viewComments.ViewName.LocalName,
                    CommentJson = json
                },
                cancellationToken
            );
        }

        private Task AddSequenceCommentsAsync(IDatabaseSequenceComments sequenceComments, CancellationToken cancellationToken)
        {
            if (sequenceComments == null)
                throw new ArgumentNullException(nameof(sequenceComments));

            var dto = Mapper.Map<IDatabaseSequenceComments, Serialization.Dto.Comments.DatabaseSequenceComments>(sequenceComments);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseCommentsQuery,
                new AddDatabaseCommentQuery
                {
                    ObjectType = ObjectTypes.Sequence,
                    ServerName = sequenceComments.SequenceName.Server,
                    DatabaseName = sequenceComments.SequenceName.Database,
                    SchemaName = sequenceComments.SequenceName.Schema,
                    LocalName = sequenceComments.SequenceName.LocalName,
                    CommentJson = json
                },
                cancellationToken
            );
        }

        private Task AddSynonymCommentsAsync(IDatabaseSynonymComments synonymComments, CancellationToken cancellationToken)
        {
            if (synonymComments == null)
                throw new ArgumentNullException(nameof(synonymComments));

            var dto = Mapper.Map<IDatabaseSynonymComments, Serialization.Dto.Comments.DatabaseSynonymComments>(synonymComments);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseCommentsQuery,
                new AddDatabaseCommentQuery
                {
                    ObjectType = ObjectTypes.Synonym,
                    ServerName = synonymComments.SynonymName.Server,
                    DatabaseName = synonymComments.SynonymName.Database,
                    SchemaName = synonymComments.SynonymName.Schema,
                    LocalName = synonymComments.SynonymName.LocalName,
                    CommentJson = json
                },
                cancellationToken
            );
        }

        private Task AddRoutineCommentsAsync(IDatabaseRoutineComments routineComments, CancellationToken cancellationToken)
        {
            if (routineComments == null)
                throw new ArgumentNullException(nameof(routineComments));

            var dto = Mapper.Map<IDatabaseRoutineComments, Serialization.Dto.Comments.DatabaseRoutineComments>(routineComments);
            var json = JsonSerializer.Serialize(dto, _settings.Value);

            return Connection.ExecuteAsync(
                InsertDatabaseCommentsQuery,
                new AddDatabaseCommentQuery
                {
                    ObjectType = ObjectTypes.Routine,
                    ServerName = routineComments.RoutineName.Server,
                    DatabaseName = routineComments.RoutineName.Database,
                    SchemaName = routineComments.RoutineName.Schema,
                    LocalName = routineComments.RoutineName.LocalName,
                    CommentJson = json
                },
                cancellationToken
            );
        }

        private const string InsertDatabaseDefaultsQuery = @$"
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

        private const string InsertDatabaseCommentsQuery = @$"
INSERT INTO database_comment (
    object_type,
    server_name,
    database_name,
    schema_name,
    local_name,
    comment_json
)
VALUES (
    @{ nameof(AddDatabaseCommentQuery.ObjectType) },
    @{ nameof(AddDatabaseCommentQuery.ServerName) },
    @{ nameof(AddDatabaseCommentQuery.DatabaseName) },
    @{ nameof(AddDatabaseCommentQuery.SchemaName) },
    @{ nameof(AddDatabaseCommentQuery.LocalName) },
    @{ nameof(AddDatabaseCommentQuery.CommentJson) }
)";
    }
}
