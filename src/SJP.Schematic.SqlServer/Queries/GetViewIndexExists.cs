using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewIndexExists
{
    internal sealed record Query : ISqlQuery<bool>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select top 1 1
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
where schema_name(v.schema_id) = @{nameof(Query.SchemaName)} and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore";
}