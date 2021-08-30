using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.SqlServer.QueryResult;

namespace SJP.Schematic.SqlServer
{
    /// <summary>
    /// A comment provider for SQL Server database routines.
    /// </summary>
    /// <seealso cref="IDatabaseRoutineProvider" />
    public class SqlServerDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabaseRoutineProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <c>null</c>.</exception>
        public SqlServerDatabaseRoutineProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
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
        /// Retrieves all database routines.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database routines.</returns>
        public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResults = await Connection.QueryAsync<GetAllRoutineNamesQueryResult>(
                RoutinesQuery,
                cancellationToken
            ).ConfigureAwait(false);

            var routineNames = queryResults
                .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
                .Select(QualifyRoutineName);

            foreach (var routineName in routineNames)
                yield return await LoadRoutineAsyncCore(routineName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// A SQL query that retrieves all database routine names.
        /// </summary>
        /// <value>A SQL query definition.</value>
        protected virtual string RoutinesQuery => RoutinesQuerySql;

        private static readonly string RoutinesQuerySql = @$"
select
    schema_name(schema_id) as [{ nameof(GetAllRoutineNamesQueryResult.SchemaName) }],
    name as [{ nameof(GetAllRoutineNamesQueryResult.RoutineName) }]
from sys.objects
where type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0
order by schema_name(schema_id), name";

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
        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
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
select top 1 schema_name(schema_id) as [{ nameof(GetRoutineNameQueryResult.SchemaName) }], name as [{ nameof(GetRoutineNameQueryResult.RoutineName) }]
from sys.objects
where schema_id = schema_id(@{ nameof(GetRoutineNameQuery.SchemaName) }) and name = @{ nameof(GetRoutineNameQuery.RoutineName) }
    and type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0";

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
        /// A SQL query that retrieves the definition of a routine.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private static readonly string DefinitionQuerySql = @$"
select m.definition
from sys.sql_modules m
inner join sys.objects o on o.object_id = m.object_id
where schema_name(o.schema_id) = @{ nameof(GetRoutineDefinitionQuery.SchemaName) } and o.name = @{ nameof(GetRoutineDefinitionQuery.RoutineName) } and o.is_ms_shipped = 0";

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

            return Connection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new GetRoutineDefinitionQuery { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
                cancellationToken
            );
        }

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
