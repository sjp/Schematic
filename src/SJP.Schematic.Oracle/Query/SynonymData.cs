namespace SJP.Schematic.Oracle.Query
{
    internal class SynonymData
    {
        public string? SchemaName { get; set; }

        public string? SynonymName { get; set; }

        public string? TargetDatabaseName { get; set; }

        public string? TargetSchemaName { get; set; }

        public string? TargetObjectName { get; set; }
    }
}