using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports column names that are almost, but not quite, a column name used widely elsewhere in the schema.
/// </summary>
/// <remarks>
/// <para>
/// No dictionary is involved. The schema is its own source of truth: a spelling that many tables
/// agree on is the intended spelling, and a near-match that appears in one table alone is the
/// outlier. That keeps the rule usable on a database full of domain jargon, abbreviations or a
/// language other than English, because the vocabulary it validates against is the one the database
/// actually uses.
/// </para>
/// <para>
/// Names are compared with their naming convention normalised away, so <c>CustomerId</c>,
/// <c>customer_id</c> and <c>customerid</c> are one name rather than three. Mixed conventions are
/// the subject of <see cref="InconsistentColumnNamingConventionRule"/> and are not reported here.
/// </para>
/// </remarks>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NearDuplicateColumnNameRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because the
    /// schema can only suggest that a name is wrong, never establish it.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    // A name used by more than one table is a name the schema has agreed on at least twice, which
    // is enough to stop it reading as a slip.
    private const int MaximumSuspectTableSupport = 1;

    // Below this many tables the suggested spelling is not established enough to argue from; two
    // tables sharing a name can just as easily be the pair that is wrong.
    private const int MinimumSuggestionTableSupport = 3;

    // The suggested spelling must also be this many times more widespread than the suspect, so
    // that two legitimately different columns of comparable standing are never ranked against
    // each other.
    private const int MinimumSupportRatio = 5;

    // One edit carries no signal between short names: id/idx, min/max, dob/doc and src/srv are all
    // a single edit apart and all deliberate.
    private const int MinimumSuspectLength = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="NearDuplicateColumnNameRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public NearDuplicateColumnNameRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a column name is a single edit away from a column name that many other tables use.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var occurrencesByName = BuildOccurrences(tables);
        var index = new DeletionNeighbourhoodIndex(occurrencesByName.Keys.ToList());

        var results = new List<(Identifier TableName, string ColumnName, string SuggestedName, int OtherTableCount)>();

        foreach (var (normalisedName, occurrences) in occurrencesByName)
        {
            if (normalisedName.Length < MinimumSuspectLength)
                continue;

            var suspectTables = occurrences.Select(static o => o.TableName).Distinct().ToList();
            if (suspectTables.Count > MaximumSuspectTableSupport)
                continue;

            var suggestion = FindSuggestion(normalisedName, suspectTables.Count, index, occurrencesByName);
            if (suggestion == null)
                continue;

            var suspectTableName = suspectTables[0];
            var otherTableCount = suggestion.TableNames.Count(t => t != suspectTableName);

            foreach (var occurrence in occurrences)
                results.Add((occurrence.TableName, occurrence.ColumnName, suggestion.DisplayName, otherTableCount));
        }

        // Ordered so that repeated runs over the same schema produce identical reports.
        var messages = results
            .OrderBy(static r => r.TableName)
            .ThenBy(static r => r.ColumnName, StringComparer.Ordinal)
            .Select(r => BuildMessage(r.TableName, r.ColumnName, r.SuggestedName, r.OtherTableCount))
            .ToList();

        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    private static Dictionary<string, List<(Identifier TableName, string ColumnName)>> BuildOccurrences(IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        var occurrencesByName = new Dictionary<string, List<(Identifier TableName, string ColumnName)>>(StringComparer.Ordinal);

        foreach (var table in tables)
        {
            foreach (var column in table.Columns)
            {
                var normalisedName = NormaliseConvention(column.Name.LocalName);
                if (normalisedName.Length == 0)
                    continue;

                if (!occurrencesByName.TryGetValue(normalisedName, out var occurrences))
                {
                    occurrences = [];
                    occurrencesByName[normalisedName] = occurrences;
                }

                occurrences.Add((table.Name, column.Name.LocalName));
            }
        }

        return occurrencesByName;
    }

    private static Suggestion? FindSuggestion(
        string suspectName,
        int suspectSupport,
        DeletionNeighbourhoodIndex index,
        Dictionary<string, List<(Identifier TableName, string ColumnName)>> occurrencesByName)
    {
        Suggestion? best = null;

        foreach (var candidateName in index.GetDistanceOneMatches(suspectName))
        {
            // A digit apart is a deliberate numbering (address1/address2), which
            // ColumnWithNumericSuffixRule already has an opinion about; an 's' apart is a
            // singular against its plural.
            if (NameSimilarity.DiffersOnlyByDigits(suspectName, candidateName)
                || NameSimilarity.DiffersOnlyByTrailingPluralS(suspectName, candidateName))
            {
                continue;
            }

            var candidateOccurrences = occurrencesByName[candidateName];
            var candidateTables = candidateOccurrences.Select(static o => o.TableName).Distinct().ToList();
            if (candidateTables.Count < MinimumSuggestionTableSupport
                || candidateTables.Count < suspectSupport * MinimumSupportRatio)
            {
                continue;
            }

            // The most widespread candidate wins, with ties broken on the normalised name so that
            // the choice does not depend on the order the tables were loaded in.
            if (best == null || IsBetterSuggestion(candidateName, candidateTables.Count, best))
                best = new Suggestion(candidateName, ChooseDisplayName(candidateOccurrences), candidateTables);
        }

        return best;
    }

    private static bool IsBetterSuggestion(string candidateName, int candidateSupport, Suggestion best)
    {
        return candidateSupport > best.TableNames.Count
            || (candidateSupport == best.TableNames.Count && string.CompareOrdinal(candidateName, best.NormalisedName) < 0);
    }

    // A normalised name may have been written several ways; the spelling shown is the most common
    // one, so that the message names something the reader will recognise.
    private static string ChooseDisplayName(List<(Identifier TableName, string ColumnName)> occurrences)
    {
        return occurrences
            .GroupBy(static o => o.ColumnName, StringComparer.Ordinal)
            .OrderByDescending(static g => g.Count())
            .ThenBy(static g => g.Key, StringComparer.Ordinal)
            .First()
            .Key;
    }

    /// <summary>
    /// Removes the naming convention from a column name, leaving only the characters that spell it.
    /// </summary>
    /// <param name="columnName">A column's local name.</param>
    /// <returns>The name lower-cased, with the characters that only separate words removed.</returns>
    private static string NormaliseConvention(string columnName)
    {
        var builder = StringBuilderCache.Acquire(columnName.Length);

        foreach (var character in columnName)
        {
            if (character is '_' or '-' or '.' || char.IsWhiteSpace(character))
                continue;

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.GetStringAndRelease();
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table holding the suspect column.</param>
    /// <param name="columnName">The name of the suspect column.</param>
    /// <param name="suggestedColumnName">The name the rest of the schema uses.</param>
    /// <param name="otherTableCount">The number of other tables using the suggested name.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="columnName"/> or <paramref name="suggestedColumnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> or <paramref name="suggestedColumnName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName, string suggestedColumnName, int otherTableCount)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedColumnName);

        var messageText = $"The column '{columnName}' in the table {tableName} is spelled almost identically to '{suggestedColumnName}', which is used by {otherTableCount.ToString(CultureInfo.InvariantCulture)} other tables. Consider whether this is a misspelling.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    private sealed record Suggestion(string NormalisedName, string DisplayName, IReadOnlyCollection<Identifier> TableNames);

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0041";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Near-duplicate column name.";
}
