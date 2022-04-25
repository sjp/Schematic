namespace SJP.Schematic.SqlServer.Queries;

internal static class GetRoutineComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;

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
    'ROUTINE' as [{ nameof(Result.ObjectType) }],
    r.name as [{ nameof(Result.ObjectName) }],
    ep.value as [{ nameof(Result.Comment) }]
from sys.objects r
left join sys.extended_properties ep on r.object_id = ep.major_id and ep.name = @{ nameof(Query.CommentProperty) } and ep.minor_id = 0
where r.schema_id = SCHEMA_ID(@{ nameof(Query.SchemaName) }) and r.name = @{ nameof(Query.RoutineName) } and r.is_ms_shipped = 0
    and r.type in ('P', 'FN', 'IF', 'TF')
";
}