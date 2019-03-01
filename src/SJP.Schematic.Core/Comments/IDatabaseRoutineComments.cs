using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseRoutineComments
    {
        Identifier RoutineName { get; }

        Option<string> Comment { get; }
    }
}
