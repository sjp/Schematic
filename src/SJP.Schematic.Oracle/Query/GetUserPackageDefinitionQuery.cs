namespace SJP.Schematic.Oracle.Query;

internal sealed record GetUserPackageDefinitionQuery
{
    public string PackageName { get; init; } = default!;
}