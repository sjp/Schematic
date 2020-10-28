namespace SJP.Schematic.Sqlite.Query
{
    internal sealed class SqliteMaster
    {
        public string type { get; set; } = default!;

        public string name { get; set; } = default!;

        public string tbl_name { get; set; } = default!;

        public long rootpage { get; set; }

        public string sql { get; set; } = default!;
    }
}
