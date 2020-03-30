using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint
{
    /// <summary>
    /// A base class for a linting rule that identifies potential issues with database objects.
    /// </summary>
    /// <seealso cref="IRule" />
    public abstract class Rule : IRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="title">A descriptive title.</param>
        /// <param name="level">The reporting level.</param>
        /// <exception cref="ArgumentNullException"><paramref name="title"/> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="level"/> is an invalid value.</exception>
        protected Rule(string title, RuleLevel level)
        {
            if (title.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(title));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));

            Level = level;
            Title = title;
        }

        /// <summary>
        /// A reporting level to use. A higher level indicates a more severe issue.
        /// </summary>
        /// <value>The reporting level.</value>
        public RuleLevel Level { get; }

        /// <summary>A title used to describe the linting rule.</summary>
        /// <value>A descriptive title.</value>
        public string Title { get; }
    }
}
