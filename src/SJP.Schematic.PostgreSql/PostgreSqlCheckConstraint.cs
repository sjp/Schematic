using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    /// <summary>
    /// A check constraint definition for use with PostgreSQL databases.
    /// </summary>
    /// <seealso cref="IDatabaseCheckConstraint" />
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class PostgreSqlCheckConstraint : IDatabaseCheckConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlCheckConstraint"/> class.
        /// </summary>
        /// <param name="checkName">The name of the check constraint, if available.</param>
        /// <param name="definition">The constraint definition.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <c>null</c>, empty or whitespace.</exception>
        public PostgreSqlCheckConstraint(Identifier checkName, string definition)
        {
            if (checkName == null)
                throw new ArgumentNullException(nameof(checkName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = Option<Identifier>.Some(checkName.LocalName);
            Definition = definition;
        }

        /// <summary>
        /// The check constraint name.
        /// </summary>
        /// <value>A constraint name, if available.</value>
        public Option<Identifier> Name { get; }

        /// <summary>
        /// The definition of the check constraint.
        /// </summary>
        /// <value>The check constraint definition.</value>
        public string Definition { get; }

        /// <summary>
        /// Indicates whether the constraint is enabled. Always <c>true</c>.
        /// </summary>
        /// <value>Always <c>true</c>.</value>
        public bool IsEnabled { get; }

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
