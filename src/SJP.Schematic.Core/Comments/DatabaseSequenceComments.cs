using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public class DatabaseSequenceComments : IDatabaseSequenceComments
    {
        public DatabaseSequenceComments(Identifier sequenceName, Option<string> comment)
        {
            SequenceName = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
            Comment = comment;
        }

        public Identifier SequenceName { get; }

        public Option<string> Comment { get; }
    }
}
