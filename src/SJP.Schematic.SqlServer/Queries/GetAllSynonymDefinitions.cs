namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSynonymDefinitions
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string SynonymName { get; init; }

        public required string? TargetServerName { get; init; }

        public required string? TargetDatabaseName { get; init; }

        public required string? TargetSchemaName { get; init; }

        public required string TargetObjectName { get; init; }
    }

    internal const string Sql = @$"
select
    s.name as [{nameof(Result.SchemaName)}],
    syn.name as [{nameof(Result.SynonymName)}],
    PARSENAME(syn.base_object_name, 4) as [{nameof(Result.TargetServerName)}],
    PARSENAME(syn.base_object_name, 3) as [{nameof(Result.TargetDatabaseName)}],
    PARSENAME(syn.base_object_name, 2) as [{nameof(Result.TargetSchemaName)}],
    PARSENAME(syn.base_object_name, 1) as [{nameof(Result.TargetObjectName)}]
from sys.synonyms syn
inner join sys.schemas s on syn.schema_id = s.schema_id
where syn.is_ms_shipped = 0
order by s.name, syn.name";
}