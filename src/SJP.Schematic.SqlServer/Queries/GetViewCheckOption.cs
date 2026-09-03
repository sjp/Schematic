using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewCheckOption
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // The check option is not present in the sys catalog views, only in INFORMATION_SCHEMA.VIEWS,
    // where SQL Server reports it as either 'CASCADE' or 'NONE'.
    internal const string Sql = @$"
select CHECK_OPTION
from INFORMATION_SCHEMA.VIEWS
where TABLE_SCHEMA = @{nameof(Query.SchemaName)} and TABLE_NAME = @{nameof(Query.ViewName)}";
}
