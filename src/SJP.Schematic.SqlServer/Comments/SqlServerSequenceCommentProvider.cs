using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer.Comments
{
    public class SqlServerSequenceCommentProvider : IDatabaseSequenceCommentProvider
    {
        public SqlServerSequenceCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected virtual string CommentProperty { get; } = "MS_Description";

        public async Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IDatabaseSequenceComments>();

            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllSequenceCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.TableName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.TableName);
                var qualifiedName = QualifySequenceName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var sequenceComment = GetFirstCommentByType(commentsData, Constants.Sequence);

                var comments = new DatabaseSequenceComments(qualifiedName, sequenceComment);
                result.Add(comments);
            }

            return result
                .OrderBy(c => c.SequenceName.Schema)
                .ThenBy(c => c.SequenceName.LocalName)
                .ToList();
        }

        protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = QualifySequenceName(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.sequences
where schema_id = schema_id(@SchemaName) and name = @SequenceName and is_ms_shipped = 0";

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceComments(candidateSequenceName, cancellationToken);
        }

        protected virtual OptionAsync<IDatabaseSequenceComments> LoadSequenceComments(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceCommentsAsyncCore(candidateSequenceName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSequenceComments>> LoadSequenceCommentsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var candidateSequenceName = QualifySequenceName(sequenceName);
            var resolvedSequenceNameOption = GetResolvedSequenceName(candidateSequenceName, cancellationToken);
            var resolvedSequenceNameOptionIsNone = await resolvedSequenceNameOption.IsNone.ConfigureAwait(false);
            if (resolvedSequenceNameOptionIsNone)
                return Option<IDatabaseSequenceComments>.None;

            var resolvedSequenceName = await resolvedSequenceNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            var commentsData = await Connection.QueryAsync<CommentsData>(
                SequenceCommentsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var sequenceComment = GetFirstCommentByType(commentsData, Constants.Sequence);

            var comments = new DatabaseSequenceComments(resolvedSequenceName, sequenceComment);
            return Option<IDatabaseSequenceComments>.Some(comments);
        }

        protected virtual string AllSequenceCommentsQuery => AllSequenceCommentsQuerySql;

        private const string AllSequenceCommentsQuerySql = @"
select SCHEMA_NAME(s.schema_id) as SchemaName, s.name as TableName, 'SEQUENCE' as ObjectType, s.name as ObjectName, ep.value as Comment
from sys.sequences s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.is_ms_shipped = 0
";

        protected virtual string SequenceCommentsQuery => SequenceCommentsQuerySql;

        private const string SequenceCommentsQuerySql = @"
select 'SEQUENCE' as ObjectType, s.name as ObjectName, ep.value as Comment
from sys.sequences s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.schema_id = SCHEMA_ID(@SchemaName) and s.name = @SequenceName and s.is_ms_shipped = 0
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentsData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => c.ObjectType == objectType)
                .Select(c => Option<string>.Some(c.Comment))
                .FirstOrDefault();
        }

        protected Identifier QualifySequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
        }

        private static class Constants
        {
            public const string Sequence = "SEQUENCE";
        }
    }
}
