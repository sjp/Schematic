namespace SJP.Schematic.SqlServer.Queries;

internal static class GetSynonymDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? TargetServerName { get; init; }

        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string TargetObjectName { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    PARSENAME(base_object_name, 4) as [{ nameof(Result.TargetServerName) }],
    PARSENAME(base_object_name, 3) as [{ nameof(Result.TargetDatabaseName) }],
    PARSENAME(base_object_name, 2) as [{ nameof(Result.TargetSchemaName) }],
    PARSENAME(base_object_name, 1) as [{ nameof(Result.TargetObjectName) }]
from sys.synonyms
where schema_id = schema_id(@{ nameof(Query.SchemaName) }) and name = @{ nameof(Query.SynonymName) } and is_ms_shipped = 0";
}