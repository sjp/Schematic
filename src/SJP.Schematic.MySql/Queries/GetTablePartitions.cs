using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTablePartitions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string PartitionName { get; init; }

        /// <summary><c>RANGE</c>, <c>LIST</c>, <c>HASH</c>, <c>KEY</c> or a <c>COLUMNS</c> variant.</summary>
        public required string PartitionMethod { get; init; }

        /// <summary>The expression rows are partitioned by, which is a column list for the <c>KEY</c> and <c>COLUMNS</c> methods.</summary>
        public required string? PartitionExpression { get; init; }
    }

    // A subpartitioned table has one row per subpartition, so partitions are grouped rather than
    // listed. MySQL partitions are named segments of the table, not tables in their own right.
    // The method and expression are the same on every row of a table.
    internal const string Sql = $"""

select
    p.partition_name as `{nameof(Result.PartitionName)}`,
    min(p.partition_method) as `{nameof(Result.PartitionMethod)}`,
    min(p.partition_expression) as `{nameof(Result.PartitionExpression)}`
from information_schema.partitions p
where p.table_schema = @{nameof(Query.SchemaName)}
    and p.table_name = @{nameof(Query.TableName)}
    and p.partition_name is not null
group by p.partition_name, p.partition_ordinal_position
order by p.partition_ordinal_position
""";
}
