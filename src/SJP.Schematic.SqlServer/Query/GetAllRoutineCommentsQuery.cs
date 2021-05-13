namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetAllRoutineCommentsQuery
    {
        public string CommentProperty { get; init; } = default!;
    }
}
