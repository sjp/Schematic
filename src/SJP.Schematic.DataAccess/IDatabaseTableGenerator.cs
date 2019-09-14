using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess
{
    public interface IDatabaseTableGenerator : IDatabaseEntityGenerator
    {
        string Generate(IRelationalDatabase database, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment);
    }
}
