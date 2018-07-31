#pragma warning disable IDE1006 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores information on collating sequences defined for the current database connection.
    /// </summary>
    public class pragma_collation_list
    {
        /// <summary>
        /// A sequence number assigned to each index for internal tracking purposes.
        /// </summary>
        public int seq { get; set; }

        /// <summary>
        /// The name of the collating function. Built-in collating functions are <c>BINARY</c>, <c>NOCASE</c>, and <c>RTRIM</c>.
        /// </summary>
        public string name { get; set; }
    }
}
#pragma warning restore IDE1006 // Naming Styles