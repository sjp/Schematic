namespace SJP.Schematic.MySql.Query
{
    internal sealed record CheckData
    {
        public string ConstraintName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public string Enforced { get; init; } = default!;
    }
}
