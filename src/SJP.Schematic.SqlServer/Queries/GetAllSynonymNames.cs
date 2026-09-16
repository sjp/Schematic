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
    s.name as [{nameof(Result.SchemaName)}],
    syn.name as [{nameof(Result.SynonymName)}]
from sys.synonyms syn
inner join sys.schemas s on syn.schema_id = s.schema_id
where syn.is_ms_shipped = 0
order by s.name, syn.name";
}