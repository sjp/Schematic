namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetPackageNameQuery
    {
        public string SchemaName { get; init; } = default!;

        public string PackageName { get; init; } = default!;
    }
}
