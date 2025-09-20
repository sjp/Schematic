using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a view is declared with an invalid definition and cannot be used.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="IViewRule"/>
public class InvalidViewDefinitionRule : Rule, IViewRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidViewDefinitionRule"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="level">The reporting level.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public InvalidViewDefinitionRule(ISchematicConnection connection, RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// A database connection.
    /// </summary>
    /// <value>The connection to the database.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Analyses database views. Reports messages when invalid view definitions are discovered on views.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        return AnalyseViewsCore(views, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseViewsCore(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken)
    {
        var messages = await views
            .Select(v => AnalyseViewAsync(v, cancellationToken))
            .ToArray()
            .WhenAll()
            .ConfigureAwait(false);

        return messages
            .SelectMany(_ => _)
            .ToArray();
    }

    /// <summary>
    /// Analyses a database view. Reports messages when the view definitions is invalid.
    /// </summary>
    /// <param name="view">A database view.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IRuleMessage>> AnalyseViewAsync(IDatabaseView view, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(view);

        return AnalyseViewAsyncCore(view, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseViewAsyncCore(IDatabaseView view, CancellationToken cancellationToken)
    {
        try
        {
            var simpleViewName = Identifier.CreateQualifiedIdentifier(view.Name.Schema, view.Name.LocalName);
            var quotedViewName = Connection.Dialect.QuoteName(simpleViewName);
            var query = "select 1 as dummy from " + quotedViewName;
            await Connection.DbConnection.ExecuteScalarAsync<long>(query, cancellationToken).ConfigureAwait(false);

            return [];
        }
        catch
        {
            var message = BuildMessage(view.Name);
            return [message];
        }
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

        var messageText = $"The view {viewName} was unable to be queried. This may indicate an incorrect view definition.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0010";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Invalid view definition.";
}