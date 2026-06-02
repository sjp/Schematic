using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a view definition selects all columns using a <c>*</c> wildcard. Such views are brittle and break when underlying columns change.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="IViewRule"/>
public partial class SelectStarInViewDefinitionRule : Rule, IViewRule
{
    [GeneratedRegex(@"\bselect\b(?:\s+distinct)?(?:\s+top\s+\d+(?:\s+percent)?)?\s+(?:[\w""\[\]]+\.)?\*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SelectStarRegex();

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectStarInViewDefinitionRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public SelectStarInViewDefinitionRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database views. Reports messages when a view selects all columns using a <c>*</c> wildcard.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        var messages = views.SelectMany(AnalyseView).ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database view. Reports a message when the view selects all columns using a <c>*</c> wildcard.
    /// </summary>
    /// <param name="view">A database view.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseView(IDatabaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        if (string.IsNullOrWhiteSpace(view.Definition) || !SelectStarRegex().IsMatch(view.Definition))
            return [];

        return [BuildMessage(view.Name)];
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="viewName">The name of the view.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var messageText = $"The view {viewName} selects all columns using a '*' wildcard. This makes the view brittle, as its result set silently changes when the underlying tables change. Consider listing columns explicitly.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0036";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "View definition selects all columns using a wildcard.";
}
