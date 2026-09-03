namespace SJP.Schematic.Oracle.Queries;

/// <summary>
/// The shape of an <c>ALL_TYPE_ATTRS</c> row describing one attribute of an object type. The 'all
/// types' and 'single type' queries project the same columns, so the provider maps both through this.
/// </summary>
internal interface IUserDefinedTypeAttributeRow
{
    string? AttributeName { get; }

    string? AttributeTypeSchema { get; }

    string? AttributeTypeName { get; }

    int DataLength { get; }

    int Precision { get; }

    int Scale { get; }

    string? Collation { get; }
}
