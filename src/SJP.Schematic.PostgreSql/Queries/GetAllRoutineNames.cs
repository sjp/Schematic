namespace SJP.Schematic.PostgreSql.Queries;

internal sealed record GetAllRoutineNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select
    ROUTINE_SCHEMA as ""{nameof(Result.SchemaName)}"",
    ROUTINE_NAME as ""{nameof(Result.RoutineName)}""
from information_schema.routines
where ROUTINE_SCHEMA not in ('pg_catalog', 'information_schema')
order by ROUTINE_SCHEMA, ROUTINE_NAME";
}