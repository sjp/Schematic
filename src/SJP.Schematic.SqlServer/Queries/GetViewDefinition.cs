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
        /// The query text of the view, or <see langword="null" /> when the view was created <c>WITH ENCRYPTION</c>
        /// or the caller lacks <c>VIEW DEFINITION</c> on it.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// Whether the view was created <c>WITH CHECK OPTION</c>.
        /// </summary>
        public required bool WithCheckOption { get; init; }

        /// <summary>
        /// Whether any DML trigger is defined on the view, i.e. whether its triggers need to be loaded.
        /// </summary>
        public required bool HasTriggers { get; init; }

        /// <summary>
        /// Whether any index is defined on the view, i.e. whether it is an indexed view whose indexes need to be loaded.
        /// </summary>
        public required bool HasIndexes { get; init; }
    }

    // The flags use the same predicates as the trigger and index queries, so those queries only need to
    // run when there is something for them to return. They read the same catalog rows, so are never stale.

    internal const string Sql = @$"
select
    sm.definition as [{nameof(Result.Definition)}],
    v.with_check_option as [{nameof(Result.WithCheckOption)}],
    cast(case when exists (
        select 1 from sys.triggers st where st.parent_id = v.object_id and st.parent_class = 1
    ) then 1 else 0 end as bit) as [{nameof(Result.HasTriggers)}],
    cast(case when exists (
        select 1 from sys.indexes i where i.object_id = v.object_id and i.is_hypothetical = 0 and i.type <> 0
    ) then 1 else 0 end as bit) as [{nameof(Result.HasIndexes)}]
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where v.schema_id = schema_id(@{nameof(Query.SchemaName)}) and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0";
}
