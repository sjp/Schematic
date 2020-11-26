using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Query;

namespace SJP.Schematic.MySql
{
    /// <summary>
    /// A MySQL database routine provider.
    /// </summary>
    /// <seealso cref="IDatabaseRoutineProvider" />
    public class MySqlDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseRoutineProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
        public MySqlDatabaseRoutineProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// A database connection that is specific to a given MySQL database.
        /// </summary>
        /// <value>A database connection.</value>
        protected ISchematicConnection Connection { get; }

        /// <summary>
        /// A database connection factory to query the database.
        /// </summary>
        /// <value>A connection factory.</value>
        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets all database routines.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database routines.</returns>
        public async IAsyncEnumerable<IDatabaseRoutine> GetAllRoutines([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await DbConnection.QueryAsync<RoutineData>(
                RoutinesQuery,
                new { SchemaName = IdentifierDefaults.Schema },
                cancellationToken
            ).ConfigureAwait(false);

            var routines = queryResult
                .Where(static row => !row.Definition.IsNullOrWhiteSpace())
                .Select(row =>
                {
                    var routineName = QualifyRoutineName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.ObjectName));
                    return new DatabaseRoutine(routineName, row.Definition!);
                });

            foreach (var routine in routines)
                yield return routine;
        }

        /// <summary>
        /// A SQL query that retrieves all database routines.
        /// </summary>
        /// <value>A SQL query definition.</value>
        protected virtual string RoutinesQuery => RoutinesQuerySql;

        private static readonly string RoutinesQuerySql = @$"
select
    ROUTINE_SCHEMA as `{ nameof(RoutineData.SchemaName) }`,
    ROUTINE_NAME as `{ nameof(RoutineData.ObjectName) }`,
    ROUTINE_DEFINITION as `{ nameof(RoutineData.Definition) }`
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName
order by ROUTINE_SCHEMA, ROUTINE_NAME";

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
            var qualifiedRoutineName = DbConnection.QueryFirstOrNone<QualifiedName>(
                RoutineNameQuery,
                new { SchemaName = candidateRoutineName.Schema, RoutineName = candidateRoutineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(candidateRoutineName.Server, candidateRoutineName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved routine name.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private static readonly string RoutineNameQuerySql = @$"
select
    ROUTINE_SCHEMA as `{ nameof(QualifiedName.SchemaName) }`,
    ROUTINE_NAME as `{ nameof(QualifiedName.ObjectName) }`
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName and ROUTINE_NAME = @RoutineName
limit 1";

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
        /// Retrieves the definition of the routine from the database.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A routine definition.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected virtual Task<string> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return DbConnection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName },
                cancellationToken
            );
        }

        /// <summary>
        /// A SQL query that retrieves the definition of a routine.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select ROUTINE_DEFINITION
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName and ROUTINE_NAME = @RoutineName";

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
