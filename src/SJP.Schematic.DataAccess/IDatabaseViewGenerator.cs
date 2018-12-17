using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public interface IDatabaseViewGenerator : IDatabaseEntityGenerator
    {
        string Generate(IDatabaseView view);
    }
}
