namespace SJP.Schematic.Core
{
    public interface IDatabaseTableIndex : IDatabaseIndex<IRelationalDatabaseTable>
    {
        IRelationalDatabaseTable Table { get; }
    }
}
