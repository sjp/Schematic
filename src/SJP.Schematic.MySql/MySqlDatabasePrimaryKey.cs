using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class MySqlDatabasePrimaryKey : MySqlDatabaseKey
    {
        public MySqlDatabasePrimaryKey(IReadOnlyCollection<IDatabaseColumn> columns)
            : base(PrimaryKeyName, DatabaseKeyType.Primary, columns)
        {
        }

        private static readonly Identifier PrimaryKeyName = Identifier.CreateQualifiedIdentifier("PRIMARY");

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DebuggerDisplay;

        private string DebuggerDisplay
        {
            get
            {
                if (Name.IsNone)
                    return "Primary Key";

                var builder = StringBuilderCache.Acquire();

                builder.Append("Primary Key");

                Name.IfSome(name =>
                {
                    builder.Append(": ")
                        .Append(name.LocalName);
                });

                return builder.GetStringAndRelease();
            }
        }
    }
}
