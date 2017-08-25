namespace SJP.Schematic.Core
{
    public interface IDatabaseViewIndex : IDatabaseIndex<IRelationalDatabaseView>
    {
        IRelationalDatabaseView View { get; }
    }
}
