using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableOptions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? Collation { get; init; }

        /// <summary><c>RANGE</c>, <c>LIST</c>, <c>HASH</c>, <c>KEY</c> or a <c>COLUMNS</c> variant. Null when the table is not partitioned.</summary>
        public required string? PartitionMethod { get; init; }

        /// <summary>The expression rows are partitioned by, which is a column list for the <c>KEY</c> and <c>COLUMNS</c> methods.</summary>
        public required string? PartitionExpression { get; init; }
    }

    // Every table has a row in information_schema.partitions; an unpartitioned one has a single row
    // whose partition columns are all null, which the ordinal position join discards.
    internal const string Sql = $"""

select
    t.table_collation as `{nameof(Result.Collation)}`,
    p.partition_method as `{nameof(Result.PartitionMethod)}`,
    p.partition_expression as `{nameof(Result.PartitionExpression)}`
from information_schema.tables t
left join information_schema.partitions p
    on p.table_schema = t.table_schema
    and p.table_name = t.table_name
    and p.partition_ordinal_position = 1
where t.table_schema = @{nameof(Query.SchemaName)}
    and t.table_name = @{nameof(Query.TableName)}
    and t.table_type = 'BASE TABLE'
""";
}
