using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

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

        public required string DataTypeName { get; init; }

        public required int CharacterMaxLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required int DateTimePrecision { get; init; }

        public required string? Collation { get; init; }

        public required string? IsNullable { get; init; }

        public required string? DefaultValue { get; init; }

        public required string? ComputedColumnDefinition { get; init; }

        public required string? ExtraInformation { get; init; }
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
where table_schema = @{nameof(Query.SchemaName)} and table_name = @{nameof(Query.ViewName)}
order by ordinal_position";
}