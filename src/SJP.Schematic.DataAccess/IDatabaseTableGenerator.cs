using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public interface IDatabaseTableGenerator : IDatabaseEntityGenerator
    {
        string Generate(IRelationalDatabaseTable table);
    }
}
