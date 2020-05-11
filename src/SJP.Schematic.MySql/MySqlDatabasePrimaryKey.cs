using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    /// <summary>
    /// A primary key that always has the name <c>PRIMARY</c>.
    /// </summary>
    /// <seealso cref="MySqlDatabaseKey" />
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class MySqlDatabasePrimaryKey : MySqlDatabaseKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabasePrimaryKey"/> class.
        /// </summary>
        /// <param name="columns">A collection of columns.</param>
        public MySqlDatabasePrimaryKey(IReadOnlyCollection<IDatabaseColumn> columns)
            : base(PrimaryKeyName, DatabaseKeyType.Primary, columns)
        {
        }

        private static readonly Identifier PrimaryKeyName = Identifier.CreateQualifiedIdentifier("PRIMARY");

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
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
