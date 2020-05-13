using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Sqlite
{
    /// <summary>
    /// A check constraint definition.
    /// </summary>
    /// <seealso cref="IDatabaseCheckConstraint" />
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class SqliteCheckConstraint : IDatabaseCheckConstraint
    {
        public SqliteCheckConstraint(Option<Identifier> checkName, string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = checkName.Map(name => Identifier.CreateQualifiedIdentifier(name.LocalName));
            Definition = definition;
        }

        public Option<Identifier> Name { get; }

        public string Definition { get; }

        public bool IsEnabled { get; } = true;

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
                var builder = StringBuilderCache.Acquire();

                builder.Append("Check");

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
