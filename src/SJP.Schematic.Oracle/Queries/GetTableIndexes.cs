namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? IndexOwner { get; init; }

        public required string? IndexName { get; init; }

        public required string? Uniqueness { get; init; }

        public required string? IsDescending { get; init; }

        public required string? ColumnName { get; init; }

        public required int ColumnPosition { get; init; }
    }

    internal const string Sql = @$"
select
    ai.OWNER as ""{nameof(Result.IndexOwner)}"",
    ai.INDEX_NAME as ""{nameof(Result.IndexName)}"",
    ai.UNIQUENESS as ""{nameof(Result.Uniqueness)}"",
    aic.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    aic.COLUMN_POSITION as ""{nameof(Result.ColumnPosition)}"",
    aic.DESCEND as ""{nameof(Result.IsDescending)}""
from SYS.ALL_INDEXES ai
inner join SYS.ALL_OBJECTS ao on ai.OWNER = ao.OWNER and ai.INDEX_NAME = ao.OBJECT_NAME
inner join SYS.ALL_IND_COLUMNS aic
    on ai.OWNER = aic.INDEX_OWNER and ai.INDEX_NAME = aic.INDEX_NAME
where ai.TABLE_OWNER = :{nameof(Query.SchemaName)} and ai.TABLE_NAME = :{nameof(Query.TableName)}
    and aic.TABLE_OWNER = :{nameof(Query.SchemaName)} and aic.TABLE_NAME = :{nameof(Query.TableName)}
    and ao.OBJECT_TYPE = 'INDEX'
order by aic.COLUMN_POSITION";
}