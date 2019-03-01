using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public class DatabaseSynonymComments : IDatabaseSynonymComments
    {
        public DatabaseSynonymComments(Identifier synonymName, Option<string> comment)
        {
            SynonymName = synonymName ?? throw new ArgumentNullException(nameof(synonymName));
            Comment = comment;
        }

        public Identifier SynonymName { get; }

        public Option<string> Comment { get; }
    }
}
