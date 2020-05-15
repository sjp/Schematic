namespace SJP.Schematic.Sqlite
{
    /// <summary>
    /// SQLite is only capable of storing a few data types. These are the possible options for representing types that use the possible storage types.
    /// </summary>
    /// <remarks>There are five possible types of data storage: <c>NULL</c>, <c>INTEGER</c>, <c>REAL</c>, <c>TEXT</c>, <c>BLOB</c>.</remarks>
    public enum SqliteTypeAffinity
    {
        /// <summary>
        /// An affinity which may contain values that use all five storage classes.
        /// </summary>
        Numeric,

        /// <summary>
        /// Behaves the same as a column with <see cref="Numeric"/> affinity. The difference between <see cref="Integer"/> and <see cref="Numeric"/> affinity is only evident in a <c>CAST</c> expression.
        /// </summary>
        Integer,

        /// <summary>
        /// A type affinity that stores all data using storage classes <c>NULL</c>, <c>TEXT</c> or <c>BLOB</c>.
        /// </summary>
        Text,

        /// <summary>
        /// Does not prefer one storage class over another and no attempt is made to coerce data from one storage class into another.
        /// </summary>
        Blob,

        /// <summary>
        /// Behaves like a column with <see cref="Numeric"/> affinity except that it forces integer values into floating point representation.
        /// </summary>
        Real
    }
}
