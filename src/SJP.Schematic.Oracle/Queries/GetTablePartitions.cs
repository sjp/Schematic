using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTablePartitions
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    // Oracle partitions are named segments of the table rather than tables in their own right, so
    // the names returned here are local to the table.
    internal const string Sql = $"""

select tp.PARTITION_NAME
from SYS.ALL_TAB_PARTITIONS tp
where tp.TABLE_OWNER = :{nameof(Query.SchemaName)} and tp.TABLE_NAME = :{nameof(Query.TableName)}
order by tp.PARTITION_POSITION
""";
}
