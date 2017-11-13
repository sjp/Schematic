namespace SJP.Schematic.Core
{
    public interface IAutoIncrement
    {
        decimal InitialValue { get; }

        decimal Increment { get; }
    }
}
