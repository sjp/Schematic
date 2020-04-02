namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines a database object that can be queried. Typically an <see cref="IRelationalDatabaseTable"/> or <see cref="IDatabaseView"/>.
    /// </summary>
    /// <seealso cref="IDatabaseEntity" />
    public interface IDatabaseQueryable : IDatabaseEntity
    {
    }
}
