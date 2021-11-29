#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores information on a tables or views in a schema.
    /// </summary>
    public sealed record pragma_table_list
    {
        /// <summary>
        /// The schema in which the table or view appears (for example <c>main</c> or <c>temp</c>).
        /// </summary>
        public string schema { get; set; } = default!;

        /// <summary>
        /// The name of the table or view.
        /// </summary>
        public string name { get; set; } = default!;

        /// <summary>
        /// The type of object. One of <c>table</c>, <c>view</c>, <c>shadow</c> (for shadow tables), or <c>virtual</c> for virtual tables.
        /// </summary>
        public string type { get; set; } = default!;

        /// <summary>
        /// The number of columns in the table, including generated columns and hidden columns.
        /// </summary>
        public int ncol { get; set; }

        /// <summary>
        /// Whether the table is a <c>WITHOUT ROWID</c> table (<c>false</c> if it is not).
        /// </summary>
        public bool wr { get; set; }

        /// <summary>
        /// Whether if the table is a STRICT table (<c>false</c> if it is not).
        /// </summary>
        public bool strict { get; set; }
    }
}
#pragma warning restore IDE1006, S101 // Naming Styles