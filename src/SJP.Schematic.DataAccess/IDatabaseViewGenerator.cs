using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess
{
    public interface IDatabaseViewGenerator : IDatabaseEntityGenerator
    {
        string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment);
    }
}
