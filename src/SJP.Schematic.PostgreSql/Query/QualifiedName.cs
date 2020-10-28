namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed class QualifiedName
    {
        public string? SchemaName { get; set; }

        public string? ObjectName { get; set; }
    }
}
