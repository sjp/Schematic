namespace SJP.Schematic.Serialization.Dto;

public class DatabaseColumn
{
    public Identifier? ColumnName { get; init; }

    public required bool IsNullable { get; init; }

    public bool IsComputed { get; init; }

    public string? DefaultValue { get; init; }

    public required DbType? Type { get; init; }

    public AutoIncrement? AutoIncrement { get; init; }

    public string? Definition { get; init; }
}