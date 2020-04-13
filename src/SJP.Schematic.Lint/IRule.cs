namespace SJP.Schematic.Lint
{
    /// <summary>
    /// Describes a rule used to analyze a database object.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// A unique identifier for a rule. Roughly equivalent to a compiler warning ID.
        /// </summary>
        /// <value>A string identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// A reporting level to use. A higher level indicates a more severe issue.
        /// </summary>
        /// <value>The reporting level.</value>
        RuleLevel Level { get; }

        /// <summary>
        /// A title used to describe the linting rule.
        /// </summary>
        /// <value>A title.</value>
        string Title { get; }
    }
}
