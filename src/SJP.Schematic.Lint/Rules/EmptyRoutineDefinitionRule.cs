using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a routine has an empty or whitespace-only definition.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="IRoutineRule"/>
public class EmptyRoutineDefinitionRule : Rule, IRoutineRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyRoutineDefinitionRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public EmptyRoutineDefinitionRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database routines. Reports messages when a routine has an empty or whitespace-only definition.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routines"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routines);

        var messages = routines.SelectMany(AnalyseRoutine).ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database routine. Reports a message when the routine has an empty or whitespace-only definition.
    /// </summary>
    /// <param name="routine">A database routine.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routine"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseRoutine(IDatabaseRoutine routine)
    {
        ArgumentNullException.ThrowIfNull(routine);

        if (!string.IsNullOrWhiteSpace(routine.Definition))
            return [];

        return [BuildMessage(routine.Name)];
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="routineName">The name of the routine.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var messageText = $"The routine {routineName} has an empty definition. Consider removing it if it is unused, or restoring its body if the definition was lost.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0037";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Routine has an empty definition.";
}
