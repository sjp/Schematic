#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores additional information on columns within an index on a table.
    /// </summary>
    public sealed record pragma_index_xinfo
    {
        /// <summary>
        /// The rank of the column within the index.
        /// </summary>
        public long seqno { get; init; }

        /// <summary>
        /// The rank of the column within the table being indexed, or <c>-1</c> if the index-column is the rowid of the table being indexed.
        /// </summary>
        public int cid { get; init; }

        /// <summary>
        /// The name of the column being indexed, or <c>NULL</c> if the index-column is the rowid of the table being indexed or an expression.
        /// </summary>
        public string? name { get; init; }

        /// <summary>
        /// Whether the index-column is sorted in reverse (descending) order
        /// </summary>
        public bool desc { get; init; }

        /// <summary>
        /// The name for the collating sequence used to compare values in the index-column.
        /// </summary>
        public string coll { get; init; } = default!;

        /// <summary>
        /// Whether the index-column is a key column. If not, it is an auxiliary/included column.
        /// </summary>
        public bool key { get; init; }
    }
}
#pragma warning restore IDE1006, S101 // Naming Styles