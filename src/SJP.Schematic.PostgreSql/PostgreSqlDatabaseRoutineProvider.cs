using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseRoutineProvider : IDatabaseRoutineProvider
    {
        public PostgreSqlDatabaseRoutineProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(
                RoutinesQuery,
                new { SchemaName = IdentifierDefaults.Schema },
                cancellationToken
            ).ConfigureAwait(false);

            var routineNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var routines = new List<IDatabaseRoutine>();

            foreach (var routineName in routineNames)
            {
                var routine = LoadRoutine(routineName, cancellationToken);
                await routine.IfSome(v => routines.Add(v)).ConfigureAwait(false);
            }

            return routines;
        }

        protected virtual string RoutinesQuery => RoutinesQuerySql;

        private const string RoutinesQuerySql = @"
select
    ROUTINE_SCHEMA as SchemaName,
    ROUTINE_NAME as ObjectName
from information_schema.routines
where ROUTINE_SCHEMA = @SchemaName
order by ROUTINE_NAME";

        public OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutine(candidateRoutineName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken = default(CancellationToken))
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

        protected virtual OptionAsync<IDatabaseRoutine> LoadRoutine(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutineAsyncCore(candidateRoutineName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseRoutine>> LoadRoutineAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var candidateRoutineName = QualifyRoutineName(routineName);
            var resolvedRoutineNameOption = GetResolvedRoutineName(candidateRoutineName, cancellationToken);
            var resolvedRoutineNameOptionIsNone = await resolvedRoutineNameOption.IsNone.ConfigureAwait(false);
            if (resolvedRoutineNameOptionIsNone)
                return Option<IDatabaseRoutine>.None;

            var resolvedRoutineName = await resolvedRoutineNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var definition = await LoadDefinitionAsync(resolvedRoutineName, cancellationToken).ConfigureAwait(false);

            var routine = new DatabaseRoutine(resolvedRoutineName, definition);
            return Option<IDatabaseRoutine>.Some(routine);
        }

        protected virtual Task<string> LoadDefinitionAsync(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return Connection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName },
                cancellationToken
            );
        }

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select ROUTINE_DEFINITION
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
