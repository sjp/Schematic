namespace SJP.Schematic.Sqlite.Pragma.Query
{
    public class pragma_index_list
    {
        public long seq { get; set; }

        public string name { get; set; }

        public bool unique { get; set; }

        public string origin { get; set; }

        public bool partial { get; set; }
    }
}
