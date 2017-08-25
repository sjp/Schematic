namespace SJP.Schematic.Core
{
    public interface IDatabaseTableColumn : IDatabaseColumn
    {
        IRelationalDatabaseTable Table { get; }
    }
}
