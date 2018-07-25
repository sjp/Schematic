namespace SJP.Schematic.PostgreSql.Query
{
    public class DatabaseMetadata
    {
        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public string DefaultSchema { get; set; }

        public string DatabaseVersion { get; set; }
    }
}
