using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal sealed record Result
    {
        public required string Definition { get; init; }

        /// <summary>
        /// The <c>sys.objects.type</c> code, i.e. one of the four values the query filters on.
        /// It is <c>char(2)</c> in the catalog, so the query trims it.
        /// </summary>
        public required string RoutineTypeCode { get; init; }
    }

    internal const string Sql = @$"
select
    m.definition as [{nameof(Result.Definition)}],
    rtrim(o.type) as [{nameof(Result.RoutineTypeCode)}]
from sys.sql_modules m
inner join sys.objects o on o.object_id = m.object_id
where o.schema_id = schema_id(@{nameof(Query.SchemaName)}) and o.name = @{nameof(Query.RoutineName)} and o.is_ms_shipped = 0
    and o.type in ('P', 'FN', 'IF', 'TF')";
}
