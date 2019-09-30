namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseCheckConstraint
    {
        public Identifier? Name { get; set; }

        public string? Definition { get; set; }

        public bool IsEnabled { get; set; }
    }
}