namespace SJP.Schematic.Serialization.Dto;

public class DbType
{
    public Identifier? TypeName { get; set; }

    public Core.DataType DataType { get; set; }

    public string? Definition { get; set; }

    public bool IsFixedLength { get; set; }

    public int MaxLength { get; set; }

    public string? ClrTypeName { get; set; }

    public NumericPrecision? NumericPrecision { get; set; }

    public Identifier? Collation { get; set; }
}
