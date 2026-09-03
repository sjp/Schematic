namespace SJP.Schematic.SqlServer.Queries;

/// <summary>
/// The shape of a <c>sys.columns</c> row that describes an attribute of a table type. The 'all
/// types' and 'single type' queries project the same columns, so the provider maps both through this.
/// </summary>
internal interface IUserDefinedTypeAttributeRow
{
    string ColumnName { get; }

    string? ColumnTypeSchema { get; }

    string ColumnTypeName { get; }

    int MaxLength { get; }

    int Precision { get; }

    int Scale { get; }

    string? Collation { get; }

    bool IsComputed { get; }

    bool IsNullable { get; }

    string? DefaultValue { get; }

    string? ComputedColumnDefinition { get; }

    bool? ComputedColumnIsPersisted { get; }

    bool IsIdentity { get; }

    long? IdentitySeed { get; }

    long? IdentityIncrement { get; }
}
