namespace SJP.Schematic.Reporting.Snapshot.Query
{
    internal record GetAllObjectNamesForTypeQuery
    {
        public string ObjectType { get; init; } = default!;
    }
}
