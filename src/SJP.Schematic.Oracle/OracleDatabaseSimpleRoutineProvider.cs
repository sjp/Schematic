﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Oracle.QueryResult;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// A routine provider for Oracle databases that access routines (but not packages).
    /// </summary>
    /// <seealso cref="IDatabaseRoutineProvider" />
    public class OracleDatabaseSimpleRoutineProvider : IDatabaseRoutineProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDatabaseSimpleRoutineProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public OracleDatabaseSimpleRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
        /// Retrieves all database routines.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database routines.</returns>
        public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<GetAllRoutinesQueryResult>(
                AllSourcesQuery,
                cancellationToken
            ).ConfigureAwait(false);

            var routines = queryResult
                .GroupBy(static r => new { r.SchemaName, r.RoutineName })
                .Select(r =>
                {
                    var name = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, r.Key.SchemaName, r.Key.RoutineName);

                    var definition = r
                        .Where(static r => r.Text != null)
                        .OrderBy(static r => r.LineNumber)
                        .Select(static r => r.Text!)
                        .Join(string.Empty);
                    var unwrappedDefinition = OracleUnwrapper.Unwrap(definition);

                    return new DatabaseRoutine(name, unwrappedDefinition);
                });

            foreach (var routine in routines)
                yield return routine;
        }

        /// <summary>
        /// Gets a query that retrieves routine information for all routines.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllSourcesQuery => AllSourcesQuerySql;

        private static readonly string AllSourcesQuerySql = @$"
SELECT
    OWNER as ""{ nameof(GetAllRoutinesQueryResult.SchemaName) }"",
    NAME as ""{ nameof(GetAllRoutinesQueryResult.RoutineName) }"",
    TYPE as ""{ nameof(GetAllRoutinesQueryResult.RoutineType) }"",
    LINE as ""{ nameof(GetAllRoutinesQueryResult.LineNumber) }"",
    TEXT as ""{ nameof(GetAllRoutinesQueryResult.Text) }""
FROM SYS.ALL_SOURCE
    WHERE TYPE in ('FUNCTION', 'PROCEDURE')
ORDER BY OWNER, NAME, LINE";

        /// <summary>
        /// Retrieves a database routine, if available.
        /// </summary>
        /// <param name="routineName">A database routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database routine in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutine(candidateRoutineName, cancellationToken);
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
        /// A SQL query that retrieves the resolved routine name.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private static readonly string RoutineNameQuerySql = @$"
select
    OWNER as ""{ nameof(GetRoutineNameQueryResult.SchemaName) }"",
    OBJECT_NAME as ""{ nameof(GetRoutineNameQueryResult.RoutineName) }""
from SYS.ALL_OBJECTS
where OWNER = :{ nameof(GetRoutineNameQuery.SchemaName) } and OBJECT_NAME = :{ nameof(GetRoutineNameQuery.RoutineName) }
    and ORACLE_MAINTAINED <> 'Y' and OBJECT_TYPE in ('FUNCTION', 'PROCEDURE')";

        /// <summary>
        /// Retrieves a routine from the database, if available.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseRoutine> LoadRoutine(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return GetResolvedRoutineName(candidateRoutineName, cancellationToken)
                .MapAsync(name => LoadRoutineAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseRoutine> LoadRoutineAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var definition = await LoadDefinitionAsync(routineName, cancellationToken).ConfigureAwait(false);
            return new DatabaseRoutine(routineName, definition);
        }

        /// <summary>
        /// Retrieves the definition of a routine.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A string representing the definition of a routine.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected virtual Task<string> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return LoadDefinitionAsyncCore(routineName, cancellationToken);
        }

        private async Task<string> LoadDefinitionAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            // fast path
            if (string.Equals(routineName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal))
                return await LoadUserDefinitionAsyncCore(routineName, cancellationToken).ConfigureAwait(false);

            var lines = await Connection.QueryAsync<string>(
                DefinitionQuery,
                new GetRoutineDefinitionQuery { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (lines.Empty())
                return string.Empty;

            var definition = lines.Join(string.Empty);
            return OracleUnwrapper.Unwrap(definition);
        }

        private async Task<string> LoadUserDefinitionAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var userLines = await Connection.QueryAsync<string>(
                UserDefinitionQuery,
                new GetUserRoutineDefinitionQuery { RoutineName = routineName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (userLines.Empty())
                return string.Empty;

            var userDefinition = userLines.Join(string.Empty);
            return OracleUnwrapper.Unwrap(userDefinition);
        }

        /// <summary>
        /// Gets a query that retrieves the routine definition for a given routine name.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private static readonly string DefinitionQuerySql = @$"
select TEXT
from SYS.ALL_SOURCE
where OWNER = :{ nameof(GetRoutineDefinitionQuery.SchemaName) } and NAME = :{ nameof(GetRoutineDefinitionQuery.RoutineName) }
    AND TYPE IN ('FUNCTION', 'PROCEDURE')
order by LINE";

        /// <summary>
        /// Gets a query that retrieves the routine definition for a given routine name in the user's schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserDefinitionQuery => UserDefinitionQuerySql;

        private static readonly string UserDefinitionQuerySql = @$"
select TEXT
from SYS.USER_SOURCE
where NAME = :{ nameof(GetUserRoutineDefinitionQuery.RoutineName) } AND TYPE IN ('FUNCTION', 'PROCEDURE')
order by LINE";

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
