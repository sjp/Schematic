using SJP.Schema.Core;

namespace SJP.Schema.Core
{
    public interface IDependentRelationalDatabase : IRelationalDatabase
    {
        IRelationalDatabase Parent { get; set; }
    }
}
