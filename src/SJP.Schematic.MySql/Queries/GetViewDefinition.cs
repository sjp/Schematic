using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

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
        /// The query text of the view.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// The view's check option, one of <c>NONE</c>, <c>LOCAL</c> or <c>CASCADED</c>.
        /// </summary>
        public required string? CheckOption { get; init; }

        /// <summary>
        /// Whether rows can be written through the view, one of <c>YES</c> or <c>NO</c>.
        /// </summary>
        public required string? IsUpdatable { get; init; }
    }

    internal const string Sql = $"""

select
    view_definition as `{nameof(Result.Definition)}`,
    check_option as `{nameof(Result.CheckOption)}`,
    is_updatable as `{nameof(Result.IsUpdatable)}`
from information_schema.views
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.ViewName)}
limit 1
""";
}
