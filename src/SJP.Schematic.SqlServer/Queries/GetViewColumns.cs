namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewColumns
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ColumnName { get; init; } = default!;

        public string? ColumnTypeSchema { get; init; }

        public string ColumnTypeName { get; init; } = default!;

        public int MaxLength { get; init; }

        public int Precision { get; init; }

        public int Scale { get; init; }

        public string? Collation { get; init; }

        public bool IsComputed { get; init; }

        public bool IsNullable { get; init; }

        public bool HasDefaultValue { get; init; }

        public string? DefaultValue { get; init; }

        public string? ComputedColumnDefinition { get; init; }

        public long? IdentitySeed { get; init; }

        public long? IdentityIncrement { get; init; }
    }

    internal const string Sql = @$"
select
    c.name as [{nameof(Result.ColumnName)}],
    schema_name(st.schema_id) as [{nameof(Result.ColumnTypeSchema)}],
    st.name as [{nameof(Result.ColumnTypeName)}],
    c.max_length as [{nameof(Result.MaxLength)}],
    c.precision as [{nameof(Result.Precision)}],
    c.scale as [{nameof(Result.Scale)}],
    c.collation_name as [{nameof(Result.Collation)}],
    c.is_computed as [{nameof(Result.IsComputed)}],
    c.is_nullable as [{nameof(Result.IsNullable)}],
    dc.parent_column_id as [{nameof(Result.HasDefaultValue)}],
    dc.definition as [{nameof(Result.DefaultValue)}],
    cc.definition as [{nameof(Result.ComputedColumnDefinition)}],
    (convert(bigint, ic.seed_value)) as [{nameof(Result.IdentitySeed)}],
    (convert(bigint, ic.increment_value)) as [{nameof(Result.IdentityIncrement)}]
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where schema_name(v.schema_id) = @{nameof(Query.SchemaName)} and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0
order by c.column_id";
}