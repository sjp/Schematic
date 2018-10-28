using System;

namespace SJP.Schematic.Core
{
    public class DatabaseSynonym : IDatabaseSynonym
    {
        public DatabaseSynonym(Identifier synonymName, Identifier targetName)
        {
            Name = synonymName ?? throw new ArgumentNullException(nameof(synonymName));
            Target = targetName ?? throw new ArgumentNullException(nameof(targetName)); // don't check for validity of target, could be a broken synonym
        }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
