namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record QualifiedName
    {
        public string? SchemaName { get; init; }

        public string? ObjectName { get; init; }
    }
}
