#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores information on key columns within an index.
    /// </summary>
    public class pragma_index_info
    {
        /// <summary>
        /// The rank of the column within the index.
        /// </summary>
        public long seqno { get; set; }

        /// <summary>
        /// The rank of the column within the table.
        /// </summary>
        public int cid { get; set; }

        /// <summary>
        /// The name of the column being indexed.
        /// </summary>
        public string? name { get; set; }
    }
}
#pragma warning restore IDE1006, S101 // Naming Styles