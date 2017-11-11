namespace SJP.Schematic.Sqlite.Pragma.Query
{
    public class pragma_index_xinfo
    {
        public long seqno { get; set; }

        public int cid { get; set; }

        public string name { get; set; }

        public bool desc { get; set; }

        public string coll { get; set; }

        public bool key { get; set; }
    }
}
