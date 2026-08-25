using System;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint;

/// <summary>
/// A rule message that describes a potential issue with a database object.
/// </summary>
/// <seealso cref="IRuleMessage" />
public class RuleMessage : IRuleMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuleMessage"/> class that is not attributable
    /// to a single database object.
    /// </summary>
    /// <param name="ruleId">The rule identifier.</param>
    /// <param name="title">The rule title.</param>
    /// <param name="level">The warning/reporting level.</param>
    /// <param name="message">A descriptive message that informs about the potential issue that was discovered.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ruleId"/> or <paramref name="title"/> or <paramref name="message"/> are <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">The given rule reporting level was not a valid value.</exception>
    public RuleMessage(string ruleId, string title, RuleLevel level, string message)
        : this(ruleId, title, level, message, Option<Identifier>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleMessage"/> class.
    /// </summary>
    /// <param name="ruleId">The rule identifier.</param>
    /// <param name="title">The rule title.</param>
    /// <param name="level">The warning/reporting level.</param>
    /// <param name="message">A descriptive message that informs about the potential issue that was discovered.</param>
    /// <param name="objectName">The name of the database object the message concerns, when attributable to one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="ruleId"/> or <paramref name="title"/> or <paramref name="message"/> are <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="ArgumentException">The given rule reporting level was not a valid value.</exception>
    public RuleMessage(string ruleId, string title, RuleLevel level, string message, Option<Identifier> objectName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (!level.IsValid())
            throw new ArgumentException($"The {nameof(RuleLevel)} provided must be a valid enum.", nameof(level));
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        RuleId = ruleId;
        Title = title;
        Level = level;
        Message = message;
        ObjectName = objectName;
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

    /// <summary>
    /// The name of the database object the message is about, when the message is attributable to
    /// a single object.
    /// </summary>
    /// <value>The name of the object the message concerns, if any.</value>
    public Option<Identifier> ObjectName { get; }
}
