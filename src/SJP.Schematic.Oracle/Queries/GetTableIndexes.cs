using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? IndexName { get; init; }

        public required string? Uniqueness { get; init; }

        /// <summary>
        /// The index structure, e.g. <c>NORMAL</c>, <c>BITMAP</c> or <c>FUNCTION-BASED NORMAL</c>.
        /// </summary>
        public required string? IndexType { get; init; }

        /// <summary>
        /// The index status, which is <c>UNUSABLE</c> for an index that the optimizer cannot use.
        /// </summary>
        public required string? Status { get; init; }

        /// <summary>
        /// Whether the optimizer is permitted to use the index, i.e. <c>VISIBLE</c> or <c>INVISIBLE</c>.
        /// </summary>
        public required string? Visibility { get; init; }

        public required string? IsDescending { get; init; }

        public required string? ColumnName { get; init; }

        public required int ColumnPosition { get; init; }
    }

    internal const string Sql = $"""

select
    ai.INDEX_NAME as "{nameof(Result.IndexName)}",
    ai.UNIQUENESS as "{nameof(Result.Uniqueness)}",
    ai.INDEX_TYPE as "{nameof(Result.IndexType)}",
    ai.STATUS as "{nameof(Result.Status)}",
    ai.VISIBILITY as "{nameof(Result.Visibility)}",
    aic.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    aic.COLUMN_POSITION as "{nameof(Result.ColumnPosition)}",
    aic.DESCEND as "{nameof(Result.IsDescending)}"
from SYS.ALL_INDEXES ai
inner join SYS.ALL_OBJECTS ao on ai.OWNER = ao.OWNER and ai.INDEX_NAME = ao.OBJECT_NAME
inner join SYS.ALL_IND_COLUMNS aic
    on ai.OWNER = aic.INDEX_OWNER and ai.INDEX_NAME = aic.INDEX_NAME
where ai.TABLE_OWNER = :{nameof(Query.SchemaName)} and ai.TABLE_NAME = :{nameof(Query.TableName)}
    and aic.TABLE_OWNER = :{nameof(Query.SchemaName)} and aic.TABLE_NAME = :{nameof(Query.TableName)}
    and ao.OBJECT_TYPE = 'INDEX'
""";
}
