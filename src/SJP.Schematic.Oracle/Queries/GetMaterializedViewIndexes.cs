using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewIndexes
{
    // The shape of an index is the same whether it is attached to a table or a materialized view, so
    // the rows are described by GetTableIndexes.Result and mapped by the same code.
    internal sealed record Query : ISqlQuery<GetTableIndexes.Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // A materialized view is backed by a container table of the same name, so its indexes are recorded
    // in ALL_INDEXES against that name, exactly as a table's are.
    internal const string Sql = $"""

select
    ai.INDEX_NAME as "{nameof(GetTableIndexes.Result.IndexName)}",
    ai.UNIQUENESS as "{nameof(GetTableIndexes.Result.Uniqueness)}",
    ai.INDEX_TYPE as "{nameof(GetTableIndexes.Result.IndexType)}",
    ai.STATUS as "{nameof(GetTableIndexes.Result.Status)}",
    ai.VISIBILITY as "{nameof(GetTableIndexes.Result.Visibility)}",
    aic.COLUMN_NAME as "{nameof(GetTableIndexes.Result.ColumnName)}",
    aic.COLUMN_POSITION as "{nameof(GetTableIndexes.Result.ColumnPosition)}",
    aic.DESCEND as "{nameof(GetTableIndexes.Result.IsDescending)}"
from SYS.ALL_INDEXES ai
inner join SYS.ALL_OBJECTS ao on ai.OWNER = ao.OWNER and ai.INDEX_NAME = ao.OBJECT_NAME
inner join SYS.ALL_IND_COLUMNS aic
    on ai.OWNER = aic.INDEX_OWNER and ai.INDEX_NAME = aic.INDEX_NAME
where ai.TABLE_OWNER = :{nameof(Query.SchemaName)} and ai.TABLE_NAME = :{nameof(Query.ViewName)}
    and aic.TABLE_OWNER = :{nameof(Query.SchemaName)} and aic.TABLE_NAME = :{nameof(Query.ViewName)}
    and ao.OBJECT_TYPE = 'INDEX'
""";
}
