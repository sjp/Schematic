#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores information on the databases attached to the current database connection.
    /// </summary>
    public class pragma_database_list
    {
        /// <summary>
        /// The order in which the databases will be queried for unqualified object names.
        /// </summary>
        public int seq { get; set; }

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The file path that the database is located at. This can be <c>null</c> for an in-memory database.
        /// </summary>
        public string file { get; set; }
    }
}
#pragma warning restore IDE1006, S101 // Naming Styles