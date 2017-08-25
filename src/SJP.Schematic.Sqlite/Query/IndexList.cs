namespace SJP.Schema.Sqlite.Query
{
    public class IndexList
    {
        public long seq { get; set; }

        public string name { get; set; }

        public bool unique { get; set; }

        public string origin { get; set; }

        public bool partial { get; set; }
    }
}
