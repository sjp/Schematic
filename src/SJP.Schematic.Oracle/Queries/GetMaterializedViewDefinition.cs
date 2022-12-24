using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select QUERY
from SYS.ALL_MVIEWS
where OWNER = :{nameof(Query.SchemaName)} and MVIEW_NAME = :{nameof(Query.ViewName)}";
}