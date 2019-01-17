using System;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DatabaseSynonym : IDatabaseSynonym
    {
        public DatabaseSynonym(Identifier synonymName, Identifier targetName)
        {
            Name = synonymName ?? throw new ArgumentNullException(nameof(synonymName));
            Target = targetName ?? throw new ArgumentNullException(nameof(targetName)); // don't check for validity of target, could be a broken synonym
        }

        public Identifier Name { get; }

        public Identifier Target { get; }

        public override string ToString() => DebuggerDisplay;

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Synonym: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append(".");

                builder.Append(Name.LocalName);

                builder.Append(" -> ");

                if (!Target.Schema.IsNullOrWhiteSpace())
                    builder.Append(Target.Schema).Append(".");

                builder.Append(Target.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
