namespace SJP.Schematic.Sqlite.Pragma.Query
{
    public class pragma_foreign_key_check
    {
        public string table { get; set; }

        public long rowid { get; set; }

        public string parent { get; set; }

        public int fkid { get; set; }
    }
}
