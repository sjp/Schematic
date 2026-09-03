using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
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

        public required bool IsComputed { get; init; }

        public required bool IsNullable { get; init; }

        public required bool HasDefaultValue { get; init; }

        public required string? DefaultValue { get; init; }

        public required string? DefaultConstraintName { get; init; }

        public required string? ComputedColumnDefinition { get; init; }

        public required bool? ComputedColumnIsPersisted { get; init; }

        public required bool IsIdentity { get; init; }

        public required long? IdentitySeed { get; init; }

        public required long? IdentityIncrement { get; init; }
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
    case when dc.object_id is null then 0 else 1 end as [{nameof(Result.HasDefaultValue)}],
    dc.definition as [{nameof(Result.DefaultValue)}],
    dc.name as [{nameof(Result.DefaultConstraintName)}],
    cc.definition as [{nameof(Result.ComputedColumnDefinition)}],
    cc.is_persisted as [{nameof(Result.ComputedColumnIsPersisted)}],
    c.is_identity as [{nameof(Result.IsIdentity)}],
    (convert(bigint, ic.seed_value)) as [{nameof(Result.IdentitySeed)}],
    (convert(bigint, ic.increment_value)) as [{nameof(Result.IdentityIncrement)}]
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
order by c.column_id";
}