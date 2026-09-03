namespace SJP.Schematic.Oracle.Queries;

/// <summary>
/// The shape of an <c>ALL_TYPES</c> row that describes a user-defined type, together with the
/// element description an <c>ALL_COLL_TYPES</c> row adds for a collection type. The 'all types' and
/// 'single type' queries project the same columns, so the provider maps both through this.
/// </summary>
internal interface IUserDefinedTypeDefinitionRow
{
    /// <summary>
    /// <c>ALL_TYPES.TYPECODE</c>, i.e. <c>OBJECT</c> or <c>COLLECTION</c>.
    /// </summary>
    string? TypeCode { get; }

    /// <summary>
    /// The schema of a collection type's element type, <see langword="null" /> for a built-in element type.
    /// </summary>
    string? ElementTypeSchema { get; }

    /// <summary>
    /// The name of a collection type's element type.
    /// </summary>
    string? ElementTypeName { get; }

    int ElementLength { get; }

    int ElementPrecision { get; }

    int ElementScale { get; }

    string? ElementCollation { get; }
}
