namespace SJP.Schematic.Core
{
    public interface IDatabaseCheckConstraint : IDatabaseOptional
    {
        Identifier Name { get; }

        string Definition { get; }
    }
}
