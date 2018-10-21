namespace SJP.Schematic.Core
{
    public interface IDatabaseComputedColumn : IDatabaseColumn
    {
        string Definition { get; }
    }
}
