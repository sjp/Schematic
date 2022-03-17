namespace SJP.Schematic.Lint;

/// <summary>
/// Describes a potential issue with a database object.
/// </summary>
public interface IRuleMessage
{
    /// <summary>
    /// The identifier of the linting rule that raised this message.
    /// </summary>
    /// <value>A unique identifier.</value>
    string RuleId { get; }

    /// <summary>
    /// The reporting level. A higher level indicates a more severe issue.
    /// </summary>
    /// <value>The reporting level.</value>
    RuleLevel Level { get; }

    /// <summary>
    /// A descriptive message describing the issue raised.
    /// </summary>
    /// <value>A descriptive message.</value>
    string Message { get; }

    /// <summary>
    /// The title of the linting rule that raised this message.
    /// </summary>
    /// <value>A descriptive title.</value>
    string Title { get; }
}
