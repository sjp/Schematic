namespace SJP.Schematic.Sqlite.Query;

internal sealed record GetTableDefinitionQuery
{
    public string TableName { get; set; } = default!;
}
