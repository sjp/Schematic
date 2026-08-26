namespace SJP.Schematic.Serialization.Dto;

public class DbType
{
    public required Identifier TypeName { get; init; }

    public required Core.DataType DataType { get; init; }

    public required string Definition { get; init; }

    public required bool IsFixedLength { get; init; }

    public required int MaxLength { get; init; }

    public string? ClrTypeName { get; init; }

    public NumericPrecision? NumericPrecision { get; init; }

    public Identifier? Collation { get; init; }
}