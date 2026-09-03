namespace SJP.Schematic.SqlServer.Queries;

/// <summary>
/// The shape of a <c>sys.types</c> row that describes a user-defined type. The 'all types' and
/// 'single type' queries project the same columns, so the provider maps both through this.
/// </summary>
internal interface IUserDefinedTypeDefinitionRow
{
    bool IsTableType { get; }

    bool IsAssemblyType { get; }

    bool IsNullable { get; }

    int MaxLength { get; }

    int Precision { get; }

    int Scale { get; }

    string? Collation { get; }

    string? BaseTypeName { get; }

    string? AssemblyName { get; }

    string? AssemblyClass { get; }

    string? DefaultValue { get; }
}
