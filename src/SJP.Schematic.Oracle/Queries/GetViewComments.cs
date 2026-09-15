using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result : IObjectCommentRow
    {
        public required string ColumnName { get; init; }

        public required string ObjectType { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = $"""

-- view
select
    'VIEW' as "{nameof(Result.ObjectType)}",
    NULL as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.ALL_TAB_COMMENTS c
where c.OWNER = :{nameof(Query.SchemaName)} and c.TABLE_NAME = :{nameof(Query.ViewName)} and c.TABLE_TYPE = 'VIEW'

union all

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    c.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.ALL_COL_COMMENTS c
where c.OWNER = :{nameof(Query.SchemaName)} and c.TABLE_NAME = :{nameof(Query.ViewName)}

""";
}