using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTablePartitionColumns
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal const string Sql = $"""

select kc.COLUMN_NAME
from SYS.ALL_PART_KEY_COLUMNS kc
where kc.OWNER = :{nameof(Query.SchemaName)} and kc.NAME = :{nameof(Query.TableName)} and kc.OBJECT_TYPE = 'TABLE'
order by kc.COLUMN_POSITION
""";
}
