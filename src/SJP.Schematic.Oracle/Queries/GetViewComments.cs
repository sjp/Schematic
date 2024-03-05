﻿using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

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
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
left join SYS.ALL_TAB_COMMENTS c on v.OWNER = c.OWNER and v.VIEW_NAME = c.TABLE_NAME and c.TABLE_TYPE = 'VIEW'
where v.OWNER = :{nameof(Query.SchemaName)} and v.VIEW_NAME = :{nameof(Query.ViewName)} and o.ORACLE_MAINTAINED <> 'Y'

union

-- columns
select
    'COLUMN' as "{nameof(Result.ObjectType)}",
    vc.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.COMMENTS as "{nameof(Result.Comment)}"
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
inner join SYS.ALL_TAB_COLS vc on vc.OWNER = v.OWNER and vc.TABLE_NAME = v.VIEW_NAME
left join SYS.ALL_COL_COMMENTS c on c.OWNER = vc.OWNER and c.TABLE_NAME = vc.TABLE_NAME and c.COLUMN_NAME = vc.COLUMN_NAME
where v.OWNER = :{nameof(Query.SchemaName)} and v.VIEW_NAME = :{nameof(Query.ViewName)} and o.ORACLE_MAINTAINED <> 'Y'

""";
}