namespace SJP.Schematic.Core
{
    public interface IDatabaseComputedColumn : IDatabaseTableColumn
    {
        string Definition { get; }
    }
}
