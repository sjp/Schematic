using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The query text of the materialized view.
        /// </summary>
        public required string? Definition { get; init; }

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

        /// <summary>
        /// Whether the materialized view's container table has any triggers, i.e. <c>Y</c> or <c>N</c>.
        /// </summary>
        public required string? HasTriggers { get; init; }
    }

    // QUERY is a LONG column, which may be selected alongside other columns of the same row.
    // HasTriggers uses the same predicates as GetMaterializedViewTriggers, so that a trigger the trigger
    // query can see is never missed by the probe.
    internal const string Sql = $"""

select
    mv.QUERY as "{nameof(Result.Definition)}",
    mv.REFRESH_MODE as "{nameof(Result.RefreshMode)}",
    mv.REFRESH_METHOD as "{nameof(Result.RefreshMethod)}",
    mv.STALENESS as "{nameof(Result.Staleness)}",
    case when exists (
        select 1
        from SYS.ALL_TRIGGERS t
        where t.TABLE_OWNER = mv.OWNER and t.TABLE_NAME = mv.MVIEW_NAME and t.BASE_OBJECT_TYPE = 'TABLE'
    ) then 'Y' else 'N' end as "{nameof(Result.HasTriggers)}"
from SYS.ALL_MVIEWS mv
where mv.OWNER = :{nameof(Query.SchemaName)} and mv.MVIEW_NAME = :{nameof(Query.ViewName)}
""";
}
