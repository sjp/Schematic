namespace SJP.Schematic.SqlServer.Query
{
    internal sealed class RoutineData
    {
        public string SchemaName { get; set; } = default!;

        public string ObjectName { get; set; } = default!;

        public string Definition { get; set; } = default!;
    }
}