using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTablePartitions
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    // A subpartitioned table has one row per subpartition, so partitions are grouped rather than
    // listed. MySQL partitions are named segments of the table, not tables in their own right.
    internal const string Sql = $"""

select p.partition_name
from information_schema.partitions p
where p.table_schema = @{nameof(Query.SchemaName)}
    and p.table_name = @{nameof(Query.TableName)}
    and p.partition_name is not null
group by p.partition_name, p.partition_ordinal_position
order by p.partition_ordinal_position
""";
}
