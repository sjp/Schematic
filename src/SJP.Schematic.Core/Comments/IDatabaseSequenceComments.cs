using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseSequenceComments
    {
        Identifier SequenceName { get; }

        Option<string> Comment { get; }
    }
}
