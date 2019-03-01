using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public class DatabaseRoutineComments : IDatabaseRoutineComments
    {
        public DatabaseRoutineComments(Identifier routineName, Option<string> comment)
        {
            RoutineName = routineName ?? throw new ArgumentNullException(nameof(routineName));
            Comment = comment;
        }

        public Identifier RoutineName { get; }

        public Option<string> Comment { get; }
    }
}
