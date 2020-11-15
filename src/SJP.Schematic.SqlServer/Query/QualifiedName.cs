namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record QualifiedName
    {
        public string SchemaName { get; init; } = default!;

        public string ObjectName { get; init; } = default!;
    }
}
