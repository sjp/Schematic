namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The shape of the <c>pg_type</c> row that describes a user-defined type. The 'all types' and
/// 'single type' queries project the same columns, so the provider maps both through this.
/// </summary>
internal interface IUserDefinedTypeDefinitionRow
{
    /// <summary>
    /// The <c>pg_type.typtype</c> of the type, i.e. <c>d</c>, <c>e</c>, <c>c</c> or <c>r</c>.
    /// </summary>
    string? TypeKind { get; }

    /// <summary>
    /// Whether a value of the type is forbidden from being <see langword="null" />, i.e. a domain
    /// declared <c>NOT NULL</c>.
    /// </summary>
    bool IsNotNull { get; }

    /// <summary>
    /// The default expression declared by a domain, else <see langword="null" />.
    /// </summary>
    string? DefaultValue { get; }

    /// <summary>
    /// For a domain, the <c>information_schema</c> name of the type it is defined over. For a range,
    /// the <c>pg_type</c> name of its subtype.
    /// </summary>
    string? BaseTypeName { get; }

    /// <summary>
    /// The schema of the type named by <see cref="BaseTypeName"/>.
    /// </summary>
    string? BaseTypeSchema { get; }

    int CharacterMaximumLength { get; }

    int NumericPrecision { get; }

    int NumericPrecisionRadix { get; }

    int NumericScale { get; }

    string? CollationName { get; }

    /// <summary>
    /// The labels of an enum type, ordered by <c>pg_enum.enumsortorder</c>, else <see langword="null" />.
    /// </summary>
    string[]? EnumLabels { get; }
}
