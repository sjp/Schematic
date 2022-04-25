namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;

        public string CommentProperty { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
-- view
select
    'VIEW' as [{ nameof(Result.ObjectType) }],
    v.name as [{ nameof(Result.ObjectName) }],
    ep.value as [{ nameof(Result.Comment) }]
from sys.views v
left join sys.extended_properties ep on v.object_id = ep.major_id and ep.name = @{ nameof(Query.CommentProperty) } and ep.minor_id = 0
where v.schema_id = SCHEMA_ID(@{ nameof(Query.SchemaName) }) and v.name = @{ nameof(Query.ViewName) } and v.is_ms_shipped = 0

union

-- columns
select
    'COLUMN' as [{ nameof(Result.ObjectType) }],
    c.name as [{ nameof(Result.ObjectName) }],
    ep.value as [{ nameof(Result.Comment) }]
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.extended_properties ep on v.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @{ nameof(Query.CommentProperty) }
where v.schema_id = SCHEMA_ID(@{ nameof(Query.SchemaName) }) and v.name = @{ nameof(Query.ViewName) } and v.is_ms_shipped = 0
";
}