namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSynonymNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string SynonymName { get; init; }
    }

    internal const string Sql = @$"
select
    schema_name(schema_id) as [{nameof(Result.SchemaName)}],
    name as [{nameof(Result.SynonymName)}]
from sys.synonyms
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}