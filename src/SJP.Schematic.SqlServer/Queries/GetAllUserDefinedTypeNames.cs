namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllUserDefinedTypeNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal const string Sql = @$"
select
    s.name as [{nameof(Result.SchemaName)}],
    t.name as [{nameof(Result.TypeName)}]
from sys.types t
inner join sys.schemas s on t.schema_id = s.schema_id
where t.is_user_defined = 1
order by s.name, t.name";
}
