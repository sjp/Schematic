namespace SJP.Schematic.Oracle.Query
{
    internal sealed record DatabaseVersion
    {
        public string? ProductName { get; init; }

        public string? VersionNumber { get; init; }
    }
}
