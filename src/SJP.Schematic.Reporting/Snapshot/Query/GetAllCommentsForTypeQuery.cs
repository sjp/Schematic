namespace SJP.Schematic.Reporting.Snapshot.Query
{
    internal record GetAllCommentsForTypeQuery
    {
        public string ObjectType { get; init; } = default!;
    }
}
