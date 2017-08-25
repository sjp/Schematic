namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseView : IDatabaseQueryable, IRelationalDatabaseViewSync, IRelationalDatabaseViewAsync
    {
        bool IsIndexed { get; }
    }
}
