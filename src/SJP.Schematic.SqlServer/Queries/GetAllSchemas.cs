namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSchemas
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string? OwnerName { get; init; }

        public required bool IsSystem { get; init; }
    }

    // schema_id 1 is dbo, 2-4 are guest/INFORMATION_SCHEMA/sys, and everything from 16384
    // upwards is the schema owned by a fixed database role (db_owner, db_datareader, ...).
    internal const string Sql = @$"
select
    s.name as [{nameof(Result.SchemaName)}],
    dp.name as [{nameof(Result.OwnerName)}],
    cast(case when s.schema_id between 2 and 4 or s.schema_id >= 16384 then 1 else 0 end as bit) as [{nameof(Result.IsSystem)}]
from sys.schemas s
left join sys.database_principals dp on s.principal_id = dp.principal_id
order by s.name";
}
