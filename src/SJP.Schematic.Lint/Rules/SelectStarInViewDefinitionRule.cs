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
    // Matches a wildcard either at the start of a select list (after any DISTINCT/ALL quantifier
    // and TOP clause) or following a comma later in the list. Requiring the '*' to be preceded by
    // whitespace, a comma, or a 'qualifier.' keeps aggregates such as count(*) from matching.
    [GeneratedRegex(
        @"\bselect\b(?:\s+(?:distinct|all))?(?:(?:\s+top\s+\d+|\s+top\s*\(\s*\d+\s*\))(?:\s+percent)?)?\s+(?:[\w""\[\]]+\.)*\*|,\s*(?:[\w""\[\]]+\.)*\*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 100)]
    private static partial Regex SelectStarRegex();

    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: warning, because
    /// SELECT * makes the view's shape change silently when a base table changes.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Warning;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectStarInViewDefinitionRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public SelectStarInViewDefinitionRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
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
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, viewName);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0036";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "View definition selects all columns using a wildcard.";
}
