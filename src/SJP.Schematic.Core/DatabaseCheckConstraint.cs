using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Represents a database check constraint.
    /// </summary>
    /// <seealso cref="IDatabaseCheckConstraint" />
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class DatabaseCheckConstraint : IDatabaseCheckConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCheckConstraint"/> class.
        /// </summary>
        /// <param name="checkName">The name of the check constraint, if available.</param>
        /// <param name="definition">The constraint definition.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <c>null</c>, empty or whitespace.</exception>
        public DatabaseCheckConstraint(Option<Identifier> checkName, string definition, bool isEnabled)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Name = checkName.Map(name => Identifier.CreateQualifiedIdentifier(name.LocalName));
            Definition = definition;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// The check constraint name.
        /// </summary>
        /// <value>A constraint name, if available.</value>
        public Option<Identifier> Name { get; }

        /// <summary>
        /// The definition of the check constraint.
        /// </summary>
        /// <value>
        /// The check constraint definition.
        /// </value>
        public string Definition { get; }

        /// <summary>
        /// Indicates whether the constraint is enabled.
        /// </summary>
        /// <value><c>true</c> if the constraint is enabled; otherwise, <c>false</c>.</value>
        public bool IsEnabled { get; }

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
