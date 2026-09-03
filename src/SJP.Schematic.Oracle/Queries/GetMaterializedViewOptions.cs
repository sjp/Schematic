using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewOptions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// When the view is refreshed, i.e. <c>DEMAND</c>, <c>COMMIT</c> or <c>NEVER</c>.
        /// </summary>
        public required string? RefreshMode { get; init; }

        /// <summary>
        /// How the view is refreshed, e.g. <c>COMPLETE</c>, <c>FAST</c>, <c>FORCE</c> or <c>NEVER</c>.
        /// </summary>
        public required string? RefreshMethod { get; init; }

        /// <summary>
        /// The relationship between the stored data and the base tables, e.g. <c>FRESH</c>, <c>STALE</c>
        /// or <c>UNUSABLE</c>. A view built <c>DEFERRED</c> is <c>UNUSABLE</c> until first refreshed.
        /// </summary>
        public required string? Staleness { get; init; }
    }

    internal const string Sql = $"""

select
    mv.REFRESH_MODE as "{nameof(Result.RefreshMode)}",
    mv.REFRESH_METHOD as "{nameof(Result.RefreshMethod)}",
    mv.STALENESS as "{nameof(Result.Staleness)}"
from SYS.ALL_MVIEWS mv
where mv.OWNER = :{nameof(Query.SchemaName)} and mv.MVIEW_NAME = :{nameof(Query.ViewName)}
""";
}
