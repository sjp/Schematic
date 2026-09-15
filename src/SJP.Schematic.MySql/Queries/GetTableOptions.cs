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
    // whose partition columns are all null, which the ordinal position filter discards.
    //
    // The partition columns are read with subqueries rather than an outer join because MariaDB only looks
    // information_schema.partitions up by schema and table name when they are constants in a where clause.
    // Through a left join, or a derived table that is merged into one, it opens every table on the server.
    // A subpartitioned table has a row for each subpartition of its first partition, hence the limit.
    internal const string Sql = $"""

select
    t.table_collation as `{nameof(Result.Collation)}`,
    (
        select p.partition_method
        from information_schema.partitions p
        where p.table_schema = @{nameof(Query.SchemaName)} and p.table_name = @{nameof(Query.TableName)}
            and p.partition_ordinal_position = 1
        limit 1
    ) as `{nameof(Result.PartitionMethod)}`,
    (
        select p.partition_expression
        from information_schema.partitions p
        where p.table_schema = @{nameof(Query.SchemaName)} and p.table_name = @{nameof(Query.TableName)}
            and p.partition_ordinal_position = 1
        limit 1
    ) as `{nameof(Result.PartitionExpression)}`
from information_schema.tables t
where t.table_schema = @{nameof(Query.SchemaName)}
    and t.table_name = @{nameof(Query.TableName)}
    and t.table_type = 'BASE TABLE'
""";
}
