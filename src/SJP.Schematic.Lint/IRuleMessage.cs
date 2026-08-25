using LanguageExt;
using SJP.Schematic.Core;

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

    /// <summary>
    /// The name of the database object the message is about, when the message is attributable to
    /// a single object. Schema-wide findings (e.g. a column type that differs across many tables)
    /// have no single owner and report <see cref="Option{A}.None"/>.
    /// </summary>
    /// <value>The name of the object the message concerns, if any.</value>
    /// <remarks>
    /// The machine-readable counterpart to the object name embedded in <see cref="Message"/>.
    /// Consumers use it to link a message back to the object that raised it — for example the
    /// HTML report, which turns it into a link to that object's page.
    /// </remarks>
    Option<Identifier> ObjectName { get; }
}
