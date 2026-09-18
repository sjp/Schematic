using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Naming;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports a word inside an object's name that looks like a misspelling of a word the rest of the schema uses consistently.
/// </summary>
/// <remarks>
/// <para>
/// There is deliberately no word list, spell-checker or language pack. The schema supplies its own
/// vocabulary: a word used across many names is correct by definition, whoever wrote it and
/// whatever language it is in. That is what makes the rule usable on a real database, where much
/// of the vocabulary is domain jargon (<c>sku</c>, <c>gtin</c>, <c>mrr</c>, <c>utm</c>) that a
/// general-purpose dictionary would flag on sight, and it means a schema with its own house
/// abbreviations simply has those abbreviations as frequent words.
/// </para>
/// <para>
/// Each <c>Analyse</c> method is its own corpus. The rule interfaces hand over one kind of object
/// at a time and provide nowhere to keep a vocabulary between calls, and carrying one over would
/// make the results depend on the order the calls were made in. Tables carry the overwhelming
/// majority of the names in any schema, so the words a view, sequence, synonym or routine name is
/// weighed against are only the words of its own kind. This is by design rather than an oversight.
/// </para>
/// <para>
/// The thresholds below were tuned against the Sakila schema used by the integration tests, whose
/// 16 tables supply 53 distinct words -- enough for the rule to run rather than stop at the corpus
/// size -- and on which it reports nothing. Sakila's closest call is <c>rate</c>, which appears in
/// <c>rental_rate</c> alone and is one edit from <c>date</c>. Requiring the suggested word in at
/// least five names, rather than the three that would be enough to call a spelling established,
/// is what keeps that quiet, and the 10x ratio keeps it quiet however large the schema grows
/// around it. The remaining Sakila pairs (<c>ai</c>/<c>au</c> and <c>id</c>/<c>idx</c>) are
/// excluded by the length floor. A rule of this kind is judged by what it reports wrongly rather
/// than by what it finds, so a threshold should only ever be loosened against a schema where the
/// result was read in full.
/// </para>
/// </remarks>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
/// <seealso cref="IViewRule" />
/// <seealso cref="ISequenceRule" />
/// <seealso cref="ISynonymRule" />
/// <seealso cref="IRoutineRule" />
public class LikelyMisspelledNameRule : Rule, ITableRule, IViewRule, ISequenceRule, ISynonymRule, IRoutineRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because the
    /// schema can only suggest that a word is wrong, never establish it.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    // Below this many distinct words there is no vocabulary to validate anything against, and
    // every threshold below becomes an argument from a handful of names. A schema of three tables
    // must produce nothing.
    private const int MinimumVocabularySize = 50;

    // A word that appears in a second name is a word the schema has written the same way twice,
    // which is enough to stop it reading as a slip.
    private const int MaximumSuspectWordNameCount = 1;

    // Below this many names the suggested word is not established enough to argue from.
    private const int MinimumSuggestionNameCount = 5;

    // The suggested word must also be this many times more widespread than the suspect. Together
    // with the count above this keeps the rule quiet on a pair of regional spellings that are both
    // in use, while still reporting one that appears once against a common other.
    private const int MinimumNameCountRatio = 10;

    // One edit carries no signal between short words: dt, qty, fk and utm are all a single edit
    // from something else and all deliberate.
    private const int MinimumSuspectWordLength = 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="LikelyMisspelledNameRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public LikelyMisspelledNameRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a table, column, index, named constraint or trigger name contains a word that looks like a misspelling.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return Task.FromResult(Analyse(tables.SelectMany(GetNamedObjects).ToList()));
    }

    /// <summary>
    /// Analyses database views. Reports messages when a view or column name contains a word that looks like a misspelling.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        return Task.FromResult(Analyse(views.SelectMany(GetNamedObjects).ToList()));
    }

    /// <summary>
    /// Analyses database sequences. Reports messages when a sequence name contains a word that looks like a misspelling.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequences"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSequences(IReadOnlyCollection<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequences);

        var namedObjects = sequences.Select(static s => new NamedObject(s.Name, "sequence", null, s.Name.LocalName)).ToList();
        return Task.FromResult(Analyse(namedObjects));
    }

    /// <summary>
    /// Analyses database synonyms. Reports messages when a synonym name contains a word that looks like a misspelling.
    /// </summary>
    /// <param name="synonyms">A set of database synonyms.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonyms"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSynonyms(IReadOnlyCollection<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonyms);

        var namedObjects = synonyms.Select(static s => new NamedObject(s.Name, "synonym", null, s.Name.LocalName)).ToList();
        return Task.FromResult(Analyse(namedObjects));
    }

    /// <summary>
    /// Analyses database routines. Reports messages when a routine name contains a word that looks like a misspelling.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routines"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routines);

        var namedObjects = routines.Select(static r => new NamedObject(r.Name, "routine", null, r.Name.LocalName)).ToList();
        return Task.FromResult(Analyse(namedObjects));
    }

    // Constraints are only named when the database named them, so an unnamed one contributes no
    // name to weigh and none to report.
    private static IReadOnlyCollection<NamedObject> GetNamedObjects(IRelationalDatabaseTable table)
    {
        var namedObjects = new List<NamedObject> { new(table.Name, "table", null, table.Name.LocalName) };

        foreach (var column in table.Columns)
            namedObjects.Add(new NamedObject(table.Name, "table", "column", column.Name.LocalName));

        foreach (var index in table.Indexes)
            namedObjects.Add(new NamedObject(table.Name, "table", "index", index.Name.LocalName));

        table.PrimaryKey
            .Bind(static pk => pk.Name)
            .Map(name => new NamedObject(table.Name, "table", "primary key", name.LocalName))
            .IfSome(namedObjects.Add);

        foreach (var uniqueKey in table.UniqueKeys)
        {
            uniqueKey.Name
                .Map(name => new NamedObject(table.Name, "table", "unique key", name.LocalName))
                .IfSome(namedObjects.Add);
        }

        foreach (var foreignKey in table.ParentKeys.Select(static fk => fk.ChildKey))
        {
            foreignKey.Name
                .Map(name => new NamedObject(table.Name, "table", "foreign key", name.LocalName))
                .IfSome(namedObjects.Add);
        }

        foreach (var check in table.Checks)
        {
            check.Name
                .Map(name => new NamedObject(table.Name, "table", "check constraint", name.LocalName))
                .IfSome(namedObjects.Add);
        }

        foreach (var trigger in table.Triggers)
            namedObjects.Add(new NamedObject(table.Name, "table", "trigger", trigger.Name.LocalName));

        return namedObjects;
    }

    private static IReadOnlyCollection<NamedObject> GetNamedObjects(IDatabaseView view)
    {
        var namedObjects = new List<NamedObject> { new(view.Name, "view", null, view.Name.LocalName) };

        foreach (var column in view.Columns)
            namedObjects.Add(new NamedObject(view.Name, "view", "column", column.Name.LocalName));

        return namedObjects;
    }

    private IReadOnlyCollection<IRuleMessage> Analyse(IReadOnlyCollection<NamedObject> namedObjects)
    {
        // Counted over distinct names rather than over occurrences, so that a name repeated in
        // fifty tables lends its words no more weight than a name written once.
        var wordsByName = namedObjects
            .Select(static o => o.Name)
            .Distinct(StringComparer.Ordinal)
            .ToDictionary(
                static name => name,
                static name => (IReadOnlyCollection<string>)NameTokenizer.Tokenize(name).Distinct(StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal
            );

        var nameCountsByWord = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var word in wordsByName.Values.SelectMany(static words => words))
            nameCountsByWord[word] = nameCountsByWord.GetValueOrDefault(word) + 1;

        if (nameCountsByWord.Count < MinimumVocabularySize)
            return [];

        var suggestionsByWord = FindSuggestions(nameCountsByWord);
        if (suggestionsByWord.Count == 0)
            return [];

        var messages = new List<IRuleMessage>();

        // Ordered so that repeated runs over the same schema produce identical reports.
        var orderedObjects = namedObjects
            .OrderBy(static o => o.ObjectName)
            .ThenBy(static o => o.MemberDescription, StringComparer.Ordinal)
            .ThenBy(static o => o.Name, StringComparer.Ordinal);

        foreach (var namedObject in orderedObjects)
        {
            // The words of a name are already distinct, so a name that repeats a suspect word
            // reports it once rather than once per occurrence.
            foreach (var word in wordsByName[namedObject.Name])
            {
                if (!suggestionsByWord.TryGetValue(word, out var suggestion))
                    continue;

                messages.Add(namedObject.MemberDescription == null
                    ? BuildObjectNameMessage(namedObject.ObjectName, namedObject.ObjectDescription, word, suggestion.Word, suggestion.NameCount)
                    : BuildMemberNameMessage(namedObject.ObjectName, namedObject.ObjectDescription, namedObject.MemberDescription, namedObject.Name, word, suggestion.Word, suggestion.NameCount));
            }
        }

        return messages;
    }

    private static Dictionary<string, Suggestion> FindSuggestions(Dictionary<string, int> nameCountsByWord)
    {
        var index = new DeletionNeighbourhoodIndex(nameCountsByWord.Keys.ToList());
        var suggestionsByWord = new Dictionary<string, Suggestion>(StringComparer.Ordinal);

        foreach (var (word, nameCount) in nameCountsByWord)
        {
            if (nameCount > MaximumSuspectWordNameCount || word.Length < MinimumSuspectWordLength)
                continue;

            // A digit run is a word no schema can agree on, so it is never the thing that is wrong.
            if (word.All(char.IsDigit))
                continue;

            Suggestion? best = null;

            foreach (var candidate in index.GetDistanceOneMatches(word))
            {
                // A singular against its plural, and a word against the same word carrying a
                // number, are conventions rather than mistakes.
                if (NameSimilarity.DiffersOnlyByTrailingPluralS(word, candidate)
                    || NameSimilarity.DiffersOnlyByDigits(word, candidate))
                {
                    continue;
                }

                var candidateNameCount = nameCountsByWord[candidate];
                if (candidateNameCount < MinimumSuggestionNameCount
                    || candidateNameCount < nameCount * MinimumNameCountRatio)
                {
                    continue;
                }

                // The most widespread candidate wins, with ties broken on the word itself so that
                // the choice does not depend on the order the objects were loaded in.
                if (best == null
                    || candidateNameCount > best.NameCount
                    || (candidateNameCount == best.NameCount && string.CompareOrdinal(candidate, best.Word) < 0))
                {
                    best = new Suggestion(candidate, candidateNameCount);
                }
            }

            if (best != null)
                suggestionsByWord[word] = best;
        }

        return suggestionsByWord;
    }

    /// <summary>
    /// Builds the message used for reporting when an object's own name contains a suspect word.
    /// </summary>
    /// <param name="objectName">The name of the object.</param>
    /// <param name="objectDescription">What kind of object it is, such as <c>table</c> or <c>view</c>.</param>
    /// <param name="suspectWord">The word that appears nowhere else in the corpus.</param>
    /// <param name="suggestedWord">The word the rest of the corpus uses.</param>
    /// <param name="suggestedWordNameCount">The number of names using the suggested word.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/>, <paramref name="objectDescription"/>, <paramref name="suspectWord"/> or <paramref name="suggestedWord"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="objectDescription"/>, <paramref name="suspectWord"/> or <paramref name="suggestedWord"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildObjectNameMessage(Identifier objectName, string objectDescription, string suspectWord, string suggestedWord, int suggestedWordNameCount)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(suspectWord);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedWord);

        var messageText = $"The name of the {objectDescription} {objectName} contains the word '{suspectWord}', which does not appear elsewhere in the schema and differs by one character from '{suggestedWord}', used in {suggestedWordNameCount.ToString(CultureInfo.InvariantCulture)} other names. Consider whether this is a misspelling.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, objectName);
    }

    /// <summary>
    /// Builds the message used for reporting when a name within an object contains a suspect word.
    /// </summary>
    /// <param name="objectName">The name of the object holding the name.</param>
    /// <param name="objectDescription">What kind of object it is, such as <c>table</c> or <c>view</c>.</param>
    /// <param name="memberDescription">What kind of name it is, such as <c>column</c> or <c>trigger</c>.</param>
    /// <param name="name">The name containing the suspect word.</param>
    /// <param name="suspectWord">The word that appears nowhere else in the corpus.</param>
    /// <param name="suggestedWord">The word the rest of the corpus uses.</param>
    /// <param name="suggestedWordNameCount">The number of names using the suggested word.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Any string argument is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildMemberNameMessage(Identifier objectName, string objectDescription, string memberDescription, string name, string suspectWord, string suggestedWord, int suggestedWordNameCount)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(suspectWord);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedWord);

        var messageText = $"The {memberDescription} name '{name}' in the {objectDescription} {objectName} contains the word '{suspectWord}', which does not appear elsewhere in the schema and differs by one character from '{suggestedWord}', used in {suggestedWordNameCount.ToString(CultureInfo.InvariantCulture)} other names. Consider whether this is a misspelling.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, objectName);
    }

    private sealed record NamedObject(Identifier ObjectName, string ObjectDescription, string? MemberDescription, string Name);

    private sealed record Suggestion(string Word, int NameCount);

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0042";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Likely misspelled word in a name.";
}
