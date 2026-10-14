using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableColumnStoreOrder
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string IndexName { get; init; }

        public required string ColumnName { get; init; }

        /// <summary>
        /// The position of the column in the sort order of an ordered columnstore index, starting at <c>1</c>.
        /// </summary>
        public required int ColumnStoreOrderOrdinal { get; init; }
    }

    /// <summary>
    /// Whether <c>sys.index_columns</c> has a <c>column_store_order_ordinal</c> column. It was added
    /// alongside ordered columnstore indexes in SQL Server 2022, and <see cref="Sql"/> cannot run without it.
    /// </summary>
    internal const string IsSupportedSql = @"
select cast(case when exists (
    select 1 from sys.all_columns
    where object_id = object_id(N'sys.index_columns') and name = N'column_store_order_ordinal'
) then 1 else 0 end as bit)";

    internal const string Sql = @$"
select
    i.name as [{nameof(Result.IndexName)}],
    c.name as [{nameof(Result.ColumnName)}],
    ic.column_store_order_ordinal as [{nameof(Result.ColumnStoreOrderOrdinal)}]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
    and i.type in (5, 6) and ic.column_store_order_ordinal > 0";
}
