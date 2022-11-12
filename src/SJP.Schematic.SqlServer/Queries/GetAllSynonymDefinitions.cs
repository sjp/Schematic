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
    schema_name(schema_id) as [{nameof(Result.SchemaName)}],
    name as [{nameof(Result.SynonymName)}],
    PARSENAME(base_object_name, 4) as [{nameof(Result.TargetServerName)}],
    PARSENAME(base_object_name, 3) as [{nameof(Result.TargetDatabaseName)}],
    PARSENAME(base_object_name, 2) as [{nameof(Result.TargetSchemaName)}],
    PARSENAME(base_object_name, 1) as [{nameof(Result.TargetObjectName)}]
from sys.synonyms
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}