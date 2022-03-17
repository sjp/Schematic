namespace SJP.Schematic.SqlServer.QueryResult;

internal sealed record GetTableColumnsQueryResult
{
    public string ColumnName { get; init; } = default!;

    public string? ColumnTypeSchema { get; init; }

    public string ColumnTypeName { get; init; } = default!;

    public int MaxLength { get; init; }

    public int Precision { get; init; }

    public int Scale { get; init; }

    public string? Collation { get; init; }

    public bool IsComputed { get; init; }

    public bool IsNullable { get; init; }

    public bool HasDefaultValue { get; init; }

    public string? DefaultValue { get; init; }

    public string? ComputedColumnDefinition { get; init; }

    public long? IdentitySeed { get; init; }

    public long? IdentityIncrement { get; init; }
}
