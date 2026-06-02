using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a sequence has been configured with values that are invalid or self-contradictory.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ISequenceRule"/>
public class InvalidSequenceConfigurationRule : Rule, ISequenceRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSequenceConfigurationRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public InvalidSequenceConfigurationRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database sequences. Reports messages when a sequence has an invalid or self-contradictory configuration.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequences"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSequences(IReadOnlyCollection<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequences);

        var messages = new List<IRuleMessage>();
        foreach (var sequence in sequences)
            messages.AddRange(AnalyseSequence(sequence));

        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database sequence. Reports messages when the sequence has an invalid or self-contradictory configuration.
    /// </summary>
    /// <param name="sequence">A database sequence.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequence"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseSequence(IDatabaseSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        var result = new List<IRuleMessage>();

        if (sequence.Increment == 0)
            result.Add(BuildMessage(sequence.Name, "has an increment of zero, so it can never advance."));

        sequence.MinValue.Bind(min => sequence.MaxValue.Map(max => (Min: min, Max: max)))
            .Where(static bounds => bounds.Min > bounds.Max)
            .IfSome(_ => result.Add(BuildMessage(sequence.Name, "has a minimum value greater than its maximum value, leaving no valid range.")));

        sequence.MinValue
            .Where(min => sequence.Start < min)
            .IfSome(_ => result.Add(BuildMessage(sequence.Name, "has a starting value below its minimum value.")));

        sequence.MaxValue
            .Where(max => sequence.Start > max)
            .IfSome(_ => result.Add(BuildMessage(sequence.Name, "has a starting value above its maximum value.")));

        if (sequence.Cycle && (sequence.MinValue.IsNone || sequence.MaxValue.IsNone))
            result.Add(BuildMessage(sequence.Name, "is set to cycle but does not define both a minimum and maximum value, so it cannot cycle and will instead overflow."));

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence.</param>
    /// <param name="reason">A description of the configuration problem.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier sequenceName, string reason)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var messageText = $"The sequence {sequenceName} {reason}";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0027";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Invalid sequence configuration.";
}
