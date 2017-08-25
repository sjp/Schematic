namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase : IRelationalDatabaseSync, IRelationalDatabaseAsync
    {
        IDatabaseDialect Dialect { get; }
    }
}
