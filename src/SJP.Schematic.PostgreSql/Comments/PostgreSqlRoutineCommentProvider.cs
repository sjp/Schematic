using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql.Comments
{
    /// <summary>
    /// A database routine comment provider for PostgreSQL.
    /// </summary>
    /// <seealso cref="IDatabaseRoutineCommentProvider" />
    public class PostgreSqlRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlRoutineCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlRoutineCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        /// <summary>
        /// A database connection factory.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets an identifier resolver that enables more relaxed matching against database object names.
        /// </summary>
        /// <value>An identifier resolver.</value>
        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// Retrieves comments for all database routines.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database routine comments, where available.</returns>
        public async IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<GetAllRoutineNamesQueryResult>(RoutinesQuery, cancellationToken).ConfigureAwait(false);
            var routineNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
                .Select(QualifyRoutineName);

            foreach (var routineName in routineNames)
                yield return await LoadRoutineCommentsAsyncCore(routineName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the resolved name of the routine. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="routineName">A routine name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A routine name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Gets the resolved name of the routine without name resolution. i.e. the name must match strictly to return a result.
        /// </summary>
        /// <param name="routineName">A routine name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A routine name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedRoutineNameStrict(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            var qualifiedRoutineName = Connection.QueryFirstOrNone<GetRoutineNameQueryResult>(
                RoutineNameQuery,
                new GetRoutineNameQuery { SchemaName = candidateRoutineName.Schema!, RoutineName = candidateRoutineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(candidateRoutineName.Server, candidateRoutineName.Database, name.SchemaName, name.RoutineName));
        }

        /// <summary>
        /// Gets a query that retrieves a resolved routine name.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private static readonly string RoutineNameQuerySql = @$"
select
    ROUTINE_SCHEMA as ""{ nameof(GetRoutineNameQueryResult.SchemaName) }"",
    ROUTINE_NAME as ""{ nameof(GetRoutineNameQueryResult.RoutineName) }""
from information_schema.routines
where ROUTINE_SCHEMA = @{ nameof(GetRoutineNameQuery.SchemaName) } and ROUTINE_NAME = @{ nameof(GetRoutineNameQuery.RoutineName) }
    and ROUTINE_SCHEMA not in ('pg_catalog', 'information_schema')
limit 1";

        /// <summary>
        /// Retrieves comments for a database routine, if available.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutineComments(candidateRoutineName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a database routine, if available.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseRoutineComments> LoadRoutineComments(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return GetResolvedRoutineName(candidateRoutineName, cancellationToken)
                .MapAsync(name => LoadRoutineCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseRoutineComments> LoadRoutineCommentsAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var commentOption = Connection.QueryFirstOrNone<GetRoutineCommentsQueryResult>(
                RoutineCommentsQuery,
                new GetRoutineCommentsQuery { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
                cancellationToken
            ).Bind(c =>
                !c.Comment.IsNullOrWhiteSpace()
                    ? OptionAsync<string>.Some(c.Comment)
                    : OptionAsync<string>.None);

            var comment = await commentOption.ToOption().ConfigureAwait(false);
            return new DatabaseRoutineComments(routineName, comment);
        }

        /// <summary>
        /// A SQL query that retrieves all database routine names.
        /// </summary>
        /// <value>A SQL query definition.</value>
        protected virtual string RoutinesQuery => RoutinesQuerySql;

        private static readonly string RoutinesQuerySql = @$"
select
    ROUTINE_SCHEMA as ""{ nameof(GetAllRoutineNamesQueryResult.SchemaName) }"",
    ROUTINE_NAME as ""{ nameof(GetAllRoutineNamesQueryResult.RoutineName) }""
from information_schema.routines
where ROUTINE_SCHEMA not in ('pg_catalog', 'information_schema')
order by ROUTINE_SCHEMA, ROUTINE_NAME";

        /// <summary>
        /// Gets a query that retrieves comments for a single routine.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineCommentsQuery => RoutineCommentsQuerySql;

        private static readonly string RoutineCommentsQuerySql = @$"
select
    d.description as ""{ nameof(GetRoutineCommentsQueryResult.Comment) }""
from pg_catalog.pg_proc p
inner join pg_namespace n on n.oid = p.pronamespace
left join pg_catalog.pg_description d on p.oid = d.objoid
where n.nspname = @{ nameof(GetRoutineCommentsQuery.SchemaName) } and p.proname = @{ nameof(GetRoutineCommentsQuery.RoutineName) }
    and n.nspname not in ('pg_catalog', 'information_schema')
";

        /// <summary>
        /// Qualifies the name of a routine, using known identifier defaults.
        /// </summary>
        /// <param name="routineName">A routine name to qualify.</param>
        /// <returns>A routine name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected Identifier QualifyRoutineName(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var schema = routineName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
        }
    }
}
