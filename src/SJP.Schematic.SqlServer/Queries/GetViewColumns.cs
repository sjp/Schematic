using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string ColumnName { get; init; }

        public required string? ColumnTypeSchema { get; init; }

        public required string ColumnTypeName { get; init; }

        public required int MaxLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }

        public required bool IsNullable { get; init; }
    }

    // Views cannot themselves own default constraints, computed columns or identity columns
    // (sys.default_constraints/sys.computed_columns/sys.identity_columns are only ever populated for
    // sys.tables), so those joins are omitted here rather than always resolving to null.
    internal const string Sql = @$"
select
    c.name as [{nameof(Result.ColumnName)}],
    schema_name(st.schema_id) as [{nameof(Result.ColumnTypeSchema)}],
    st.name as [{nameof(Result.ColumnTypeName)}],
    c.max_length as [{nameof(Result.MaxLength)}],
    c.precision as [{nameof(Result.Precision)}],
    c.scale as [{nameof(Result.Scale)}],
    c.collation_name as [{nameof(Result.Collation)}],
    c.is_nullable as [{nameof(Result.IsNullable)}]
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.types st on c.user_type_id = st.user_type_id
where v.schema_id = schema_id(@{nameof(Query.SchemaName)}) and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0
order by c.column_id";
}
