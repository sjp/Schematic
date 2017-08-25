using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled
{
    public interface IDependentRelationalDatabase : IRelationalDatabase
    {
        IRelationalDatabase Parent { get; set; }
    }
}
