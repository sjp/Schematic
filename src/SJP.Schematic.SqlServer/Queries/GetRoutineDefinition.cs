using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select m.definition
from sys.sql_modules m
inner join sys.objects o on o.object_id = m.object_id
where o.schema_id = schema_id(@{nameof(Query.SchemaName)}) and o.name = @{nameof(Query.RoutineName)} and o.is_ms_shipped = 0
    and o.type in ('P', 'FN', 'IF', 'TF')";
}