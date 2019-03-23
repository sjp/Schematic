using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Query;

namespace SJP.Schematic.MySql.Comments
{
    public class MySqlRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        public MySqlRoutineCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public async Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IDatabaseRoutineComments>();

            var commentsData = await Connection.QueryAsync<CommentsData>(
                AllRoutineCommentsQuery,
                new { SchemaName = IdentifierDefaults.Schema },
                cancellationToken
            ).ConfigureAwait(false);

            foreach (var comment in commentsData)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(comment.SchemaName, comment.ObjectName);
                var qualifiedName = QualifyRoutineName(tmpIdentifier);

                var routineComment = !comment.Comment.IsNullOrWhiteSpace()
                    ? Option<string>.Some(comment.Comment)
                    : Option<string>.None;

                var comments = new DatabaseRoutineComments(qualifiedName, routineComment);
                result.Add(comments);
            }

            return result;
        }

        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            routineName = QualifyRoutineName(routineName);
            var qualifiedRoutineName = Connection.QueryFirstOrNone<QualifiedName>(
                RoutineNameQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(routineName.Server, routineName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private const string RoutineNameQuerySql = @"
select
    ROUTINE_SCHEMA as SchemaName,
    ROUTINE_NAME as ObjectName
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName and ROUTINE_NAME = @RoutineName
limit 1";

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutineComments(candidateRoutineName, cancellationToken);
        }

        protected virtual OptionAsync<IDatabaseRoutineComments> LoadRoutineComments(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutineCommentsAsyncCore(candidateRoutineName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseRoutineComments>> LoadRoutineCommentsAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var candidateRoutineName = QualifyRoutineName(routineName);
            var resolvedRoutineNameOption = GetResolvedRoutineName(candidateRoutineName, cancellationToken);
            var resolvedRoutineNameOptionIsNone = await resolvedRoutineNameOption.IsNone.ConfigureAwait(false);
            if (resolvedRoutineNameOptionIsNone)
                return Option<IDatabaseRoutineComments>.None;

            var resolvedRoutineName = await resolvedRoutineNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            var comment = await Connection.ExecuteScalarAsync<string>(
                RoutineCommentQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var routineComment = !comment.IsNullOrWhiteSpace()
                ? Option<string>.Some(comment)
                : Option<string>.None;

            var comments = new DatabaseRoutineComments(resolvedRoutineName, routineComment);
            return Option<IDatabaseRoutineComments>.Some(comments);
        }

        protected virtual string AllRoutineCommentsQuery => AllRoutineCommentsQuerySql;

        private const string AllRoutineCommentsQuerySql = @"
select
    ROUTINE_SCHEMA as SchemaName,
    ROUTINE_NAME as ObjectName,
    ROUTINE_COMMENT as Comment
from INFORMATION_SCHEMA.ROUTINES
where ROUTINE_SCHEMA = @SchemaName
order by ROUTINE_SCHEMA, ROUTINE_NAME";

        protected virtual string RoutineCommentQuery => RoutineCommentQuerySql;

        private const string RoutineCommentQuerySql = @"
select ROUTINE_COMMENT
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName and ROUTINE_NAME = @RoutineName";

        protected Identifier QualifyRoutineName(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var schema = routineName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
        }
    }
}
