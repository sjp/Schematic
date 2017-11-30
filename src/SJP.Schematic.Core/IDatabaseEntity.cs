namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines behaviour for all objects within a database.
    /// </summary>
    public interface IDatabaseEntity
    {
        /// <summary>
        /// The name of the database object.
        /// </summary>
        Identifier Name { get; }
    }
}
