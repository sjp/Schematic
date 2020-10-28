namespace SJP.Schematic.Oracle.Query
{
    internal sealed class DatabaseHost
    {
        public string? ServerHost { get; set; }

        public string? ServerSid { get; set; }

        public string? DatabaseName { get; set; }

        public string? DefaultSchema { get; set; }
    }
}
