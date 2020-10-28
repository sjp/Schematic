namespace SJP.Schematic.SqlServer.Query
{
    internal sealed class QualifiedName
    {
        public string SchemaName { get; set; } = default!;

        public string ObjectName { get; set; } = default!;
    }
}
