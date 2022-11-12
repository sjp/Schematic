namespace SJP.Schematic.Serialization.Dto;

public class DbType
{
    public Identifier? TypeName { get; init; }

    public required Core.DataType DataType { get; init; }

    public string? Definition { get; init; }

    public required bool IsFixedLength { get; init; }

    public required int MaxLength { get; init; }

    public string? ClrTypeName { get; init; }

    public NumericPrecision? NumericPrecision { get; init; }

    public Identifier? Collation { get; init; }
}