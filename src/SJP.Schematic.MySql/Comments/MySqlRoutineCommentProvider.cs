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
using SJP.Schematic.MySql.Query;
using SJP.Schematic.MySql.QueryResult;

namespace SJP.Schematic.MySql.Comments
{
    /// <summary>
    /// A routine comment provider for MySQL routines.
    /// </summary>
    /// <seealso cref="IDatabaseRoutineCommentProvider" />
    public class MySqlRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlRoutineCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the given database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <c>null</c>.</exception>
        public MySqlRoutineCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection factory to query the database.
        /// </summary>
        /// <value>A connection factory.</value>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Retrieves comments for all database routines.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database routine comments, where available.</returns>
        public async IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResults = await Connection.QueryAsync<GetAllRoutineNamesQueryResult>(
                RoutinesQuery,
                new GetAllRoutineNamesQuery { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            ).ConfigureAwait(false);

            var routineNames = queryResults
                .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
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
        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            routineName = QualifyRoutineName(routineName);
            var qualifiedRoutineName = Connection.QueryFirstOrNone<GetRoutineNameQueryResult>(
                RoutineNameQuery,
                new GetRoutineNameQuery { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(routineName.Server, routineName.Database, name.SchemaName, name.RoutineName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved routine name.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private static readonly string RoutineNameQuerySql = @$"
select
    ROUTINE_SCHEMA as `{ nameof(GetRoutineNameQueryResult.SchemaName) }`,
    ROUTINE_NAME as `{ nameof(GetRoutineNameQueryResult.RoutineName) }`
from information_schema.routines
where ROUTINE_SCHEMA = @{ nameof(GetRoutineNameQuery.SchemaName) } and ROUTINE_NAME = @{ nameof(GetRoutineNameQuery.RoutineName) }
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
        /// Retrieves a routine's comments.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for a routine, if available.</returns>
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
            var comment = await Connection.ExecuteScalarAsync<string>(
                RoutineCommentQuery,
                new GetRoutineCommentsQuery { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var routineComment = !comment.IsNullOrWhiteSpace()
                ? Option<string>.Some(comment)
                : Option<string>.None;

            return new DatabaseRoutineComments(routineName, routineComment);
        }

        /// <summary>
        /// A SQL query definition that retrieves all routine names in a given database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutinesQuery => RoutinesQuerySql;

        private static readonly string RoutinesQuerySql = @$"
select
    ROUTINE_SCHEMA as `{ nameof(GetAllRoutineNamesQueryResult.SchemaName) }`,
    ROUTINE_NAME as `{ nameof(GetAllRoutineNamesQueryResult.RoutineName) }`
from information_schema.routines
where ROUTINE_SCHEMA = @{ nameof(GetAllRoutineNamesQuery.SchemaName) }
order by ROUTINE_SCHEMA, ROUTINE_NAME";

        /// <summary>
        /// A SQL query that retrieves the definition of a routine.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineCommentQuery => RoutineCommentQuerySql;

        private static readonly string RoutineCommentQuerySql = @$"
select ROUTINE_COMMENT
from information_schema.routines
where ROUTINE_SCHEMA = @{ nameof(GetRoutineCommentsQuery.SchemaName) } and ROUTINE_NAME = @{ nameof(GetRoutineCommentsQuery.RoutineName) }";

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
