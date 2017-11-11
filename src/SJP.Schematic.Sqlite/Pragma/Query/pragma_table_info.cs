namespace SJP.Schematic.Sqlite.Pragma.Query
{
    public class pragma_table_info
    {
        public int cid { get; set; }

        public string name { get; set; }

        public string type { get; set; }

        public bool notnull { get; set; }

        public string dflt_value { get; set; }

        public int pk { get; set; }
    }
}
