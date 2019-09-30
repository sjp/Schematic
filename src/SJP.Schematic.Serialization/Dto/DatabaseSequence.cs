namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseSequence
    {
        public Identifier? Name { get; set; }

        public int Cache { get; set; }

        public bool Cycle { get; set; }

        public decimal Increment { get; set; }

        public decimal? MaxValue { get; set; }

        public decimal? MinValue { get; set; }

        public decimal Start { get; set; }
    }
}
