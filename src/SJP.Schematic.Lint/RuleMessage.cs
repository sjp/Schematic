using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint;

/// <summary>
/// A rule message that describes a potential issue with a database object.
/// </summary>
/// <seealso cref="IRuleMessage" />
public class RuleMessage : IRuleMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuleMessage"/> class.
    /// </summary>
    /// <param name="ruleId">The rule identifier.</param>
    /// <param name="title">The rule title.</param>
    /// <param name="level">The warning/reporting level.</param>
    /// <param name="message">A descriptive message that informs about the potential issue that was discovered.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ruleId"/> or <paramref name="title"/> or <paramref name="message"/> are <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">The given rule reporting level was not a valid value.</exception>
    public RuleMessage(string ruleId, string title, RuleLevel level, string message)
    {
        if (ruleId.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(ruleId));
        if (title.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(title));
        if (!level.IsValid())
            throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));
        if (message.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(message));

        RuleId = ruleId;
        Title = title;
        Level = level;
        Message = message;
    }

    /// <summary>
    /// The identifier of the linting rule that raised this message.
    /// </summary>
    /// <value>A unique identifier.</value>
    public string RuleId { get; }

    /// <summary>
    /// The title of the linting rule that raised this message.
    /// </summary>
    /// <value>A descriptive title.</value>
    public string Title { get; }

    /// <summary>
    /// The reporting level. A higher level indicates a more severe issue.
    /// </summary>
    /// <value>The reporting level.</value>
    public RuleLevel Level { get; }

    /// <summary>
    /// A descriptive message describing the issue raised.
    /// </summary>
    /// <value>A descriptive message.</value>
    public string Message { get; }
}