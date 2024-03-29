﻿using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserMaterializedViewComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string ViewName { get; init; }
    }

    internal sealed record Result
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
from SYS.USER_MVIEWS v
left join SYS.USER_MVIEW_COMMENTS c on v.MVIEW_NAME = c.MVIEW_NAME
where v.MVIEW_NAME = :{nameof(Query.ViewName)}

union

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    vc.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.USER_MVIEWS v
inner join SYS.USER_TAB_COLS vc on vc.TABLE_NAME = v.MVIEW_NAME
left join SYS.USER_COL_COMMENTS c on c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.MVIEW_NAME = :{nameof(Query.ViewName)}

""";
}