namespace SJP.Schematic.Oracle.Query
{
    internal sealed class DatabaseHost
    {
        public string? ServerHost { get; init; }

        public string? ServerSid { get; init; }

        public string? DatabaseName { get; init; }

        public string? DefaultSchema { get; init; }
    }
}
