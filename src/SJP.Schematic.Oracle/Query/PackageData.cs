namespace SJP.Schematic.Oracle.Query
{
    internal sealed record PackageData
    {
        public string? SourceType { get; init; }

        public int LineNumber { get; init; }

        public string? Text { get; init; }
    }
}