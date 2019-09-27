using System;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class DatabaseRoutine : IDatabaseRoutine
    {
        public DatabaseRoutine(Identifier routineName, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = routineName ?? throw new ArgumentNullException(nameof(routineName));
            Definition = definition;
        }

        public Identifier Name { get; }

        public string Definition { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DebuggerDisplay;

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Routine: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append('.');

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
