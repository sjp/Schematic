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

        /// <summary>
        /// The sort order of the index column, i.e. <c>ASC</c> or <c>DESC</c>.
        /// </summary>
        public required string? Descend { get; init; }

        /// <summary>
        /// The name of the indexed column. For a descending or function-based index column this is a
        /// system-generated hidden column, e.g. <c>SYS_NC00004$</c>.
        /// </summary>
        public required string? ColumnName { get; init; }

        /// <summary>
        /// The expression of a descending or function-based index column, which is a quoted column name
        /// such as <c>"A"</c> for a descending column. <see langword="null" /> for any other column.
        /// </summary>
        public required string? ColumnExpression { get; init; }

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
    aic.DESCEND as "{nameof(Result.Descend)}",
    aie.COLUMN_EXPRESSION as "{nameof(Result.ColumnExpression)}"
from SYS.ALL_INDEXES ai
inner join SYS.ALL_IND_COLUMNS aic
    on ai.OWNER = aic.INDEX_OWNER and ai.INDEX_NAME = aic.INDEX_NAME
left join SYS.ALL_IND_EXPRESSIONS aie
    on aic.INDEX_OWNER = aie.INDEX_OWNER and aic.INDEX_NAME = aie.INDEX_NAME and aic.COLUMN_POSITION = aie.COLUMN_POSITION
where ai.TABLE_OWNER = :{nameof(Query.SchemaName)} and ai.TABLE_NAME = :{nameof(Query.TableName)}
    and aic.TABLE_OWNER = :{nameof(Query.SchemaName)} and aic.TABLE_NAME = :{nameof(Query.TableName)}
""";
}
