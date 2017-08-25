namespace SJP.Schematic.Core
{
    public interface IDatabaseViewColumn : IDatabaseColumn
    {
        IRelationalDatabaseView View { get; }
    }
}
