using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string IndexName { get; init; }

        public required bool IsNonUnique { get; init; }

        public required int ColumnOrdinal { get; init; }

        /// <summary>
        /// The indexed column, which is <see langword="null" /> for a functional index column.
        /// </summary>
        public required string? ColumnName { get; init; }

        /// <summary>
        /// The expression defining a functional index column, otherwise <see langword="null" />.
        /// </summary>
        public required string? Expression { get; init; }

        /// <summary>
        /// How the column is sorted within the index. <c>A</c> for ascending, <c>D</c> for descending,
        /// and <see langword="null" /> when the index is unsorted.
        /// </summary>
        public required string? ColumnSort { get; init; }

        /// <summary>
        /// The number of indexed characters when only a prefix of the column is indexed.
        /// </summary>
        public required int? PrefixLength { get; init; }

        /// <summary>
        /// The index structure, e.g. <c>BTREE</c> or <c>FULLTEXT</c>.
        /// </summary>
        public required string? IndexType { get; init; }

        /// <summary>
        /// Whether the optimizer is permitted to use the index, i.e. <c>YES</c> or <c>NO</c>.
        /// </summary>
        public required string? IsVisible { get; init; }
    }

    internal const string Sql = $"""

select
    index_name as `{nameof(Result.IndexName)}`,
    non_unique as `{nameof(Result.IsNonUnique)}`,
    seq_in_index as `{nameof(Result.ColumnOrdinal)}`,
    column_name as `{nameof(Result.ColumnName)}`,
    expression as `{nameof(Result.Expression)}`,
    collation as `{nameof(Result.ColumnSort)}`,
    sub_part as `{nameof(Result.PrefixLength)}`,
    index_type as `{nameof(Result.IndexType)}`,
    is_visible as `{nameof(Result.IsVisible)}`
from information_schema.statistics
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}
order by index_name, seq_in_index
""";
}