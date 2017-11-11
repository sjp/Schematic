namespace SJP.Schematic.Sqlite.Pragma.Query
{
    public class pragma_wal_checkpoint
    {
        public int busy { get; set; }

        public int log { get; set; }

        public int checkpointed { get; set; }
    }
}
