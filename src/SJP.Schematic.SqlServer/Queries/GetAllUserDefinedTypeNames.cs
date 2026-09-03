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
    schema_name(schema_id) as [{nameof(Result.SchemaName)}],
    name as [{nameof(Result.TypeName)}]
from sys.types
where is_user_defined = 1
order by schema_name(schema_id), name";
}
