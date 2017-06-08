using SJP.Schema.Core;

namespace SJP.Schema.Modelled
{
    public interface IDependentRelationalDatabase : IRelationalDatabase
    {
        IRelationalDatabase Parent { get; set; }
    }
}
