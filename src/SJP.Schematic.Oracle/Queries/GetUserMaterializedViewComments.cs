using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserMaterializedViewComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
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
from SYS.USER_MVIEW_COMMENTS c
where c.MVIEW_NAME = :{nameof(Query.ViewName)}

union all

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    c.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.USER_COL_COMMENTS c
where c.TABLE_NAME = :{nameof(Query.ViewName)}

""";
}