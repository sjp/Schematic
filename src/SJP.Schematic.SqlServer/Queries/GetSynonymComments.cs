namespace SJP.Schematic.SqlServer.Queries;

internal static class GetSynonymComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;

        public string CommentProperty { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
select
    'SYNONYM' as [{nameof(Result.ObjectType)}],
    s.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.synonyms s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)} and ep.minor_id = 0
where s.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and s.name = @{nameof(Query.SynonymName)} and s.is_ms_shipped = 0
";
}