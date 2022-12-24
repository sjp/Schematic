using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select view_definition
from information_schema.views
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.ViewName)}";
}