using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        public PostgreSqlRoutineCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(AllRoutineCommentsQuery, cancellationToken).ConfigureAwait(false);

            var comments = allCommentsData
                .Select(c =>
                {
                    var qualifiedName = QualifyRoutineName(Identifier.CreateQualifiedIdentifier(c.SchemaName, c.ObjectName));

                    var routineComment = !c.Comment.IsNullOrWhiteSpace()
                        ? Option<string>.Some(c.Comment)
                        : Option<string>.None;
                    return new DatabaseRoutineComments(qualifiedName, routineComment);
                });

            foreach (var comment in comments)
                yield return comment;
        }

        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(routineName)
                .Select(QualifyRoutineName);

            return resolvedNames
                .Select(name => GetResolvedRoutineNameStrict(name, cancellationToken))
                .FirstSome(cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedRoutineNameStrict(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            var qualifiedRoutineName = Connection.QueryFirstOrNone<QualifiedName>(
                RoutineNameQuery,
                new { SchemaName = candidateRoutineName.Schema, RoutineName = candidateRoutineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(candidateRoutineName.Server, candidateRoutineName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private const string RoutineNameQuerySql = @"
select
    ROUTINE_SCHEMA as SchemaName,
    ROUTINE_NAME as ObjectName
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName and ROUTINE_NAME = @RoutineName
    and ROUTINE_SCHEMA not in ('pg_catalog', 'information_schema')
limit 1";

        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
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
            return GetResolvedRoutineName(candidateRoutineName, cancellationToken)
                .Bind(name =>
                {
                    return Connection.QueryFirstOrNone<CommentsData>(
                        RoutineCommentsQuery,
                        new { SchemaName = name.Schema, RoutineName = name.LocalName },
                        cancellationToken
                    ).Map<IDatabaseRoutineComments>(c =>
                    {
                        var routineComment = !c.Comment.IsNullOrWhiteSpace()
                            ? Option<string>.Some(c.Comment)
                            : Option<string>.None;

                        return new DatabaseRoutineComments(name, routineComment);
                    });
                });
        }

        protected virtual string AllRoutineCommentsQuery => AllRoutineCommentsQuerySql;

        private const string AllRoutineCommentsQuerySql = @"
select n.nspname as SchemaName, p.proname as ObjectName, d.description as Comment
from pg_catalog.pg_proc p
inner join pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_description d on p.oid = d.objoid
where n.nspname not in ('pg_catalog', 'information_schema')
order by n.nspname, p.proname
";

        protected virtual string RoutineCommentsQuery => RoutineCommentsQuerySql;

        private const string RoutineCommentsQuerySql = @"
select n.nspname as SchemaName, p.proname as ObjectName, d.description as Comment
from pg_catalog.pg_proc p
inner join pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_description d on p.oid = d.objoid
where n.nspname = @SchemaName and p.proname = @RoutineName
    and n.nspname not in ('pg_catalog', 'information_schema')
";

        protected Identifier QualifyRoutineName(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var schema = routineName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
        }
    }
}
