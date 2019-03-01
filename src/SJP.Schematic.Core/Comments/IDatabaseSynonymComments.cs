using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IDatabaseSynonymComments
    {
        Identifier SynonymName { get; }

        Option<string> Comment { get; }
    }
}
