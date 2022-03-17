namespace SJP.Schematic.Sqlite.Query;

internal sealed record GetSqliteMasterQuery
{
    public string TableName { get; set; } = default!;
}
