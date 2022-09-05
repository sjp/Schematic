namespace SJP.Schematic.MySql.Queries;

internal static class GetTableColumns
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ColumnName { get; init; } = default!;

        public string DataTypeName { get; init; } = default!;

        public int CharacterMaxLength { get; init; }

        public int Precision { get; init; }

        public int Scale { get; init; }

        public int DateTimePrecision { get; init; }

        public string? Collation { get; init; }

        public string? IsNullable { get; init; }

        public string? DefaultValue { get; init; }

        public string? ComputedColumnDefinition { get; init; }

        public string? ExtraInformation { get; init; }
    }

    internal const string Sql = @$"
select
    column_name as `{nameof(Result.ColumnName)}`,
    data_type as `{nameof(Result.DataTypeName)}`,
    character_maximum_length as `{nameof(Result.CharacterMaxLength)}`,
    numeric_precision as `{nameof(Result.Precision)}`,
    numeric_scale as `{nameof(Result.Scale)}`,
    datetime_precision as `{nameof(Result.DateTimePrecision)}`,
    collation_name as `{nameof(Result.Collation)}`,
    is_nullable as `{nameof(Result.IsNullable)}`,
    column_default as `{nameof(Result.DefaultValue)}`,
    generation_expression as `{nameof(Result.ComputedColumnDefinition)}`,
    extra as `{nameof(Result.ExtraInformation)}`
from information_schema.columns
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.TableName)}
order by ordinal_position";
}