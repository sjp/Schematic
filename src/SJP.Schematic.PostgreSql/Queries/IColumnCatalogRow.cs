namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The columns that are shared by the table, view and materialized view column queries, which
/// all read them through <see cref="ColumnCatalogSql"/>.
/// </summary>
internal interface IColumnCatalogRow
{
    string? ColumnName { get; }

    int OrdinalPosition { get; }

    string? ColumnDefault { get; }

    string? IsNullable { get; }

    string? DataType { get; }

    int CharacterMaximumLength { get; }

    int NumericPrecision { get; }

    int NumericPrecisionRadix { get; }

    int NumericScale { get; }

    int? DatetimePrecision { get; }

    string? CollationCatalog { get; }

    string? CollationSchema { get; }

    string? CollationName { get; }

    string? DomainSchema { get; }

    string? DomainName { get; }

    string? UdtSchema { get; }

    string? UdtName { get; }

    string? TypeKind { get; }

    string? ElementTypeSchema { get; }

    string? ElementTypeName { get; }

    string? ElementTypeKind { get; }

    string[]? EnumLabels { get; }
}
