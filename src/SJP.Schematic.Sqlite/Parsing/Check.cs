using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    /// <summary>
    /// A parsed check constraint definition.
    /// </summary>
    public class Check
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Check"/> class.
        /// </summary>
        /// <param name="constraintName">A parsed constraint name.</param>
        /// <param name="definition">The check definition.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <c>null</c>, or has no tokens.</exception>
        public Check(Option<string> constraintName, IEnumerable<Token<SqliteToken>> definition)
        {
            if (definition == null || definition.Empty())
                throw new ArgumentNullException(nameof(definition));

            Name = constraintName;
            Definition = definition.ToList();
        }

        /// <summary>
        /// The parsed check constraint name.
        /// </summary>
        /// <value>A constraint name.</value>
        public Option<string> Name { get; }

        /// <summary>
        /// The parsed check constraint definition.
        /// </summary>
        /// <value>A check constraint definition.</value>
        public IEnumerable<Token<SqliteToken>> Definition { get; }
    }
}
