namespace SJP.Schematic.Core
{
    public interface IDependentRelationalDatabase : IRelationalDatabase
    {
        IRelationalDatabase Parent { get; set; }
    }
}
