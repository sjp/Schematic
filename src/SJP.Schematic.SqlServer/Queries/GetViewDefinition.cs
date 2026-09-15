using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The query text of the view, or <see langword="null" /> when the view was created <c>WITH ENCRYPTION</c>.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// Whether the view was created <c>WITH CHECK OPTION</c>.
        /// </summary>
        public required bool WithCheckOption { get; init; }
    }

    internal const string Sql = @$"
select
    sm.definition as [{nameof(Result.Definition)}],
    v.with_check_option as [{nameof(Result.WithCheckOption)}]
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where v.schema_id = schema_id(@{nameof(Query.SchemaName)}) and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0";
}
