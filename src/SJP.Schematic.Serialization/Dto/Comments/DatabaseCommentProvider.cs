using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto.Comments
{
    public class DatabaseCommentProvider
    {
        public IdentifierDefaults IdentifierDefaults { get; set; } = default!;

        public IIdentifierResolutionStrategy? IdentifierResolver { get; set; }

        public IEnumerable<DatabaseTableComments> TableComments { get; set; } = Array.Empty<DatabaseTableComments>();

        public IEnumerable<DatabaseViewComments> ViewComments { get; set; } = Array.Empty<DatabaseViewComments>();

        public IEnumerable<DatabaseSequenceComments> SequenceComments { get; set; } = Array.Empty<DatabaseSequenceComments>();

        public IEnumerable<DatabaseSynonymComments> SynonymComments { get; set; } = Array.Empty<DatabaseSynonymComments>();

        public IEnumerable<DatabaseRoutineComments> RoutineComments { get; set; } = Array.Empty<DatabaseRoutineComments>();
    }
}
