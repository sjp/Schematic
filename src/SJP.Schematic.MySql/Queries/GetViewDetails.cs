using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetViewDetails
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    /// <summary>
    /// Either the view's definition row, whose <see cref="Ordinal"/> is <see langword="null" />, or one of its columns.
    /// </summary>
    internal sealed record Result
    {
        /// <summary>
        /// The query text of the view. Only set on the definition row.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// The view's check option, one of <c>NONE</c>, <c>LOCAL</c> or <c>CASCADED</c>. Only set on the definition row.
        /// </summary>
        public required string? CheckOption { get; init; }

        /// <summary>
        /// Whether rows can be written through the view, one of <c>YES</c> or <c>NO</c>. Only set on the definition row.
        /// </summary>
        public required string? IsUpdatable { get; init; }

        /// <summary>
        /// The column's position in the view, or <see langword="null" /> on the definition row.
        /// </summary>
        public required long? Ordinal { get; init; }

        public required string? ColumnName { get; init; }

        public required string? DataTypeName { get; init; }

        /// <summary>
        /// The full column type as declared, e.g. <c>enum('a','b')</c>, <c>tinyint(1)</c> or <c>int unsigned</c>. Only this reports the members of an enum or set, the display width, and whether the type is unsigned.
        /// </summary>
        public required string? ColumnType { get; init; }

        /// <summary>
        /// The column's maximum length in characters.
        /// </summary>
        public required long? CharacterMaxLength { get; init; }

        public required int? Precision { get; init; }

        public required int? Scale { get; init; }

        /// <summary>
        /// The fractional seconds precision of a temporal type, or <see langword="null"/> for any other type.
        /// </summary>
        public required int? DateTimePrecision { get; init; }

        public required string? Collation { get; init; }

        public required string? IsNullable { get; init; }

        public required string? DefaultValue { get; init; }

        public required string? ExtraInformation { get; init; }
    }

    // The definition and the columns come from different catalog tables, so they are combined with union all
    // rather than joined. Each branch filters on a constant schema and view name, which is what lets MariaDB
    // look its catalog table up by key: through a join, even a left join on constants, MariaDB fills
    // information_schema.columns for every database on the server. A join would also drop a view whose base
    // table has been dropped, which has a definition but no columns.
    //
    // The definition row is the one with a null ordinal; every column row has one.
    internal const string Sql = $"""

select
    v.view_definition as `{nameof(Result.Definition)}`,
    v.check_option as `{nameof(Result.CheckOption)}`,
    v.is_updatable as `{nameof(Result.IsUpdatable)}`,
    null as `{nameof(Result.Ordinal)}`,
    null as `{nameof(Result.ColumnName)}`,
    null as `{nameof(Result.DataTypeName)}`,
    null as `{nameof(Result.ColumnType)}`,
    null as `{nameof(Result.CharacterMaxLength)}`,
    null as `{nameof(Result.Precision)}`,
    null as `{nameof(Result.Scale)}`,
    null as `{nameof(Result.DateTimePrecision)}`,
    null as `{nameof(Result.Collation)}`,
    null as `{nameof(Result.IsNullable)}`,
    null as `{nameof(Result.DefaultValue)}`,
    null as `{nameof(Result.ExtraInformation)}`
from information_schema.views v
where v.table_schema = @{nameof(Query.SchemaName)} and v.table_name = @{nameof(Query.ViewName)}
union all
select
    null,
    null,
    null,
    c.ordinal_position,
    c.column_name,
    c.data_type,
    c.column_type,
    c.character_maximum_length,
    c.numeric_precision,
    c.numeric_scale,
    c.datetime_precision,
    c.collation_name,
    c.is_nullable,
    c.column_default,
    c.extra
from information_schema.columns c
where c.table_schema = @{nameof(Query.SchemaName)} and c.table_name = @{nameof(Query.ViewName)}
""";
}
