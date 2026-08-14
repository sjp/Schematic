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
        var invalidViewNamesByChunk = await views
            .Chunk(ProbeBatchSize)
            .Select(chunk => FindInvalidViewNamesAsync(chunk, cancellationToken))
            .ToArray()
            .WhenAll();

        return invalidViewNamesByChunk
            .SelectMany(names => names)
            .Select(BuildMessage)
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
        var invalidViewNames = await FindInvalidViewNamesAsync([view], cancellationToken);
        return invalidViewNames
            .Select(BuildMessage)
            .ToArray();
    }

    /// <summary>
    /// Determines which views in a batch have invalid definitions.
    /// </summary>
    /// <remarks>
    /// Probes the whole batch in a single statement. When that fails, some view in the batch is
    /// invalid but the probe alone cannot say which one, so the batch is split in half and each half is
    /// probed independently, recursing until individual invalid views are isolated. A healthy batch
    /// (the overwhelmingly common case) costs a single round trip regardless of its size.
    /// </remarks>
    /// <param name="views">A batch of database views, all belonging to the same rule invocation.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>The names of any views in <paramref name="views"/> which have an invalid definition.</returns>
    private async Task<IReadOnlyCollection<Identifier>> FindInvalidViewNamesAsync(IReadOnlyList<IDatabaseView> views, CancellationToken cancellationToken)
    {
        var probeQuery = BuildProbeQuery(views);
        var isValid = await IsProbeQueryValidAsync(probeQuery, cancellationToken);
        if (isValid)
            return [];

        if (views.Count == 1)
            return [views[0].Name];

        var midpoint = views.Count / 2;
        var firstHalf = views.Take(midpoint).ToArray();
        var secondHalf = views.Skip(midpoint).ToArray();

        var (firstHalfInvalidNames, secondHalfInvalidNames) = await (
            FindInvalidViewNamesAsync(firstHalf, cancellationToken),
            FindInvalidViewNamesAsync(secondHalf, cancellationToken)
        ).WhenAll();

        return [.. firstHalfInvalidNames, .. secondHalfInvalidNames];
    }

    /// <summary>
    /// Builds a query that succeeds only when every view in <paramref name="views"/> can be queried.
    /// </summary>
    /// <param name="views">A batch of database views.</param>
    /// <returns>A query combining a no-op probe of every view via <c>union all</c>.</returns>
    private string BuildProbeQuery(IReadOnlyList<IDatabaseView> views)
    {
        return views
            .Select(v => Identifier.CreateQualifiedIdentifier(v.Name.Schema, v.Name.LocalName))
            .Select(Connection.Dialect.QuoteName)
            .Select(static quotedViewName => "select 1 as dummy from " + quotedViewName + " where 1 = 0")
            .Join(" union all ");
    }

    /// <summary>
    /// Determines whether a probe query executes successfully.
    /// </summary>
    /// <param name="probeQuery">A probe query, as built by <see cref="BuildProbeQuery(IReadOnlyList{IDatabaseView})"/>.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns><see langword="true" /> if the probe query executed without error; otherwise <see langword="false" />.</returns>
    private async Task<bool> IsProbeQueryValidAsync(string probeQuery, CancellationToken cancellationToken)
    {
        try
        {
            await Connection.ConnectionFactory.ExecuteScalarAsync<long>(probeQuery, cancellationToken);
            return true;
        }
        // A cancellation must propagate rather than being reported as an invalid view definition.
        catch (Exception ex) when (ex is not OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    /// <summary>
    /// The maximum number of views probed by a single query. Chosen conservatively to stay well within
    /// every supported dialect's limits on statement length and <c>union</c> branch count.
    /// </summary>
    private const int ProbeBatchSize = 32;

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