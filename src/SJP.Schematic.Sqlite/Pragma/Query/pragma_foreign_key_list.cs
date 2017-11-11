namespace SJP.Schematic.Sqlite.Pragma.Query
{
    public class pragma_foreign_key_list
    {
        public long id { get; set; }

        public int seq { get; set; }

        public string table { get; set; }

        public string from { get; set; }

        public string to { get; set; }

        public string on_update { get; set; }

        public string on_delete { get; set; }

        public string match { get; set; }
    }
}
