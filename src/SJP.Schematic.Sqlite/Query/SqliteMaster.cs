namespace SJP.Schematic.Sqlite.Query
{
    public class SqliteMaster
    {
        public string type { get; set; }

        public string name { get; set; }

        public string tbl_name { get; set; }

        public long rootpage { get; set; }

        public string sql { get; set; }
    }
}
