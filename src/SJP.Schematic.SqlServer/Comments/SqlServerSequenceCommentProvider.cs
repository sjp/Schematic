using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public async IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllSequenceCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var sequenceComments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.TableName })
                .Select(g => new
                {
                    Name = QualifySequenceName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName)),
                    Comments = g.ToList()
                })
                .Select(g =>
                {
                    var sequenceComment = GetFirstCommentByType(g.Comments, Constants.Sequence);
                    return new DatabaseSequenceComments(g.Name, sequenceComment);
                })
                .OrderBy(c => c.SequenceName.Schema)
                .ThenBy(c => c.SequenceName.LocalName);

            foreach (var sequenceComment in sequenceComments)
                yield return sequenceComment;
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

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
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
            return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
                .MapAsync(name => LoadSequenceCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseSequenceComments> LoadSequenceCommentsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var commentsData = await Connection.QueryAsync<CommentsData>(
                SequenceCommentsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var sequenceComment = GetFirstCommentByType(commentsData, Constants.Sequence);

            return new DatabaseSequenceComments(sequenceName, sequenceComment);
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
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
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
