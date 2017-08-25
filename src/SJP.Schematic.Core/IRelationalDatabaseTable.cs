namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTable : IDatabaseQueryable, IRelationalDatabaseTableSync, IRelationalDatabaseTableAsync
    {
    }
}
