namespace SJP.Schematic.SqlServer.Query
{
    internal class SynonymData
    {
        public string SchemaName { get; set; } = default!;

        public string ObjectName { get; set; } = default!;

        public string? TargetServerName { get; set; }

        public string? TargetDatabaseName { get; set; }

        public string? TargetSchemaName { get; set; }

        public string TargetObjectName { get; set; } = default!;
    }
}