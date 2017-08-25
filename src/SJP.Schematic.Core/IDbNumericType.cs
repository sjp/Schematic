namespace SJP.Schematic.Core
{
    public interface IDbNumericType : IDbType
    {
        int Precision { get; }

        int Scale { get; }
    }
}
