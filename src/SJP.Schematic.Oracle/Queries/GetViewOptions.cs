using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewOptions
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// Whether the view was created <c>WITH READ ONLY</c>, i.e. <c>Y</c> or <c>N</c>.
        /// </summary>
        public required string? IsReadOnly { get; init; }

        /// <summary>
        /// Whether any of the view's columns can be written through, i.e. <c>Y</c> or <c>N</c>.
        /// </summary>
        public required string? IsUpdatable { get; init; }

        /// <summary>
        /// Whether the view was created <c>WITH CHECK OPTION</c>, i.e. <c>Y</c> or <c>N</c>.
        /// </summary>
        public required string? HasCheckOption { get; init; }
    }

    // Oracle records a view's WITH CHECK OPTION as a constraint of type 'V' on the view, and reports
    // per-column writability through ALL_UPDATABLE_COLUMNS. A view that is not read only is still only
    // updatable when Oracle can map a write back onto a single base table, so the column view is
    // consulted rather than inferring updatability from READ_ONLY alone.
    internal const string Sql = $"""

select
    v.READ_ONLY as "{nameof(Result.IsReadOnly)}",
    case when exists (
        select 1
        from SYS.ALL_UPDATABLE_COLUMNS uc
        where uc.OWNER = v.OWNER and uc.TABLE_NAME = v.VIEW_NAME
            and (uc.UPDATABLE = 'YES' or uc.INSERTABLE = 'YES' or uc.DELETABLE = 'YES')
    ) then 'Y' else 'N' end as "{nameof(Result.IsUpdatable)}",
    case when exists (
        select 1
        from SYS.ALL_CONSTRAINTS c
        where c.OWNER = v.OWNER and c.TABLE_NAME = v.VIEW_NAME and c.CONSTRAINT_TYPE = 'V'
    ) then 'Y' else 'N' end as "{nameof(Result.HasCheckOption)}"
from SYS.ALL_VIEWS v
where v.OWNER = :{nameof(Query.SchemaName)} and v.VIEW_NAME = :{nameof(Query.ViewName)}
""";
}
