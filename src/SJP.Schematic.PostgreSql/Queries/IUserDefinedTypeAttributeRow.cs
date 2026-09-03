namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The shape of an <c>information_schema.attributes</c> row describing one attribute of a composite
/// type. The 'all types' and 'single type' queries project the same columns, so the provider maps
/// both through this.
/// </summary>
internal interface IUserDefinedTypeAttributeRow
{
    string? AttributeName { get; }

    string? DataType { get; }

    string? UdtSchema { get; }

    string? UdtName { get; }

    /// <summary>
    /// The <c>pg_type.typtype</c> of the attribute's type.
    /// </summary>
    string? TypeKind { get; }

    string? ElementTypeSchema { get; }

    string? ElementTypeName { get; }

    string? ElementTypeKind { get; }

    string[]? EnumLabels { get; }

    int CharacterMaximumLength { get; }

    int NumericPrecision { get; }

    int NumericPrecisionRadix { get; }

    int NumericScale { get; }

    string? CollationName { get; }

    string? IsNullable { get; }

    string? AttributeDefault { get; }
}
