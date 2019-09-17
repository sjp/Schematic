namespace SJP.Schematic.PostgreSql.Query
{
    internal class ColumnDataWithIdentity : ColumnData
    {
        /// <summary>
        /// If the column is an identity column, then <c>YES</c>, else <c>NO</c>.
        /// </summary>
        public string is_identity { get; set; }

        /// <summary>
        /// If the column is an identity column, then <c>ALWAYS</c> or <c>BY DEFAULT</c>, reflecting the definition of the column.
        /// </summary>
        public string identity_generation { get; set; }

        /// <summary>
        /// If the column is an identity column, then the start value of the internal sequence, else <c>null</c>.
        /// </summary>
        public string identity_start { get; set; }

        /// <summary>
        /// If the column is an identity column, then the increment of the internal sequence, else <c>null</c>.
        /// </summary>
        public string identity_increment { get; set; }

        /// <summary>
        /// If the column is an identity column, then the maximum value of the internal sequence, else <c>null</c>.
        /// </summary>
        public string identity_maximum { get; set; }

        /// <summary>
        /// 	If the column is an identity column, then the minimum value of the internal sequence, else <c>null</c>.
        /// </summary>
        public string identity_minimum { get; set; }

        /// <summary>
        /// If the column is an identity column, then <c>YES</c> if the internal sequence cycles or <c>NO</c> if it does not; otherwise <c>null</c>.
        /// </summary>
        public string identity_cycle { get; set; }
    }
}
