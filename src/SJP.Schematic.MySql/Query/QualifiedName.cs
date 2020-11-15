namespace SJP.Schematic.MySql.Query
{
    internal sealed record QualifiedName
    {
        public string SchemaName { get; init; } = default!;

        public string ObjectName { get; init; } = default!;
    }
}
