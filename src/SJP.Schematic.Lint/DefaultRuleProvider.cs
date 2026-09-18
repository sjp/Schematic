using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint;

/// <summary>
/// Constructs a rule provider that returns a default set of rules.
/// </summary>
/// <seealso cref="IRuleProvider" />
public class DefaultRuleProvider : IRuleProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRuleProvider"/> class, whose rules query the
    /// database for anything the schema does not tell them.
    /// </summary>
    public DefaultRuleProvider()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRuleProvider"/> class.
    /// </summary>
    /// <param name="tableStatistics">The statistics the database records for its tables, given to the rules that can use them in place of a query. <see langword="null" /> when none are available.</param>
    public DefaultRuleProvider(ITableStatisticsProvider? tableStatistics)
        : this(tableStatistics, queryDatabase: true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRuleProvider"/> class, optionally leaving out every rule that runs its own queries against the database.
    /// </summary>
    /// <param name="tableStatistics">The statistics the database records for its tables, given to the rules that can use them in place of a query. <see langword="null" /> when none are available.</param>
    /// <param name="queryDatabase"><see langword="true" /> to include the rules that query the database, which read table data or execute views; <see langword="false" /> to return only the rules that work from the schema metadata alone.</param>
    /// <remarks>Leaving those rules out avoids table scans on large databases, but the issues only they can find (empty tables, columns that never hold a value, self-referencing rows and views that no longer compile) go unreported.</remarks>
    public DefaultRuleProvider(ITableStatisticsProvider? tableStatistics, bool queryDatabase)
    {
        TableStatistics = tableStatistics;
        QueryDatabase = queryDatabase;
    }

    /// <summary>
    /// The statistics the database records for its tables, if any were supplied.
    /// </summary>
    /// <value>A table statistics provider.</value>
    protected ITableStatisticsProvider? TableStatistics { get; }

    /// <summary>
    /// Whether the rules that run their own queries against the database are included.
    /// </summary>
    /// <value><see langword="true" /> if rules that read table data or execute views are provided; otherwise <see langword="false" />.</value>
    protected bool QueryDatabase { get; }

    /// <summary>
    /// Retrieves the default set of rules used to analyze database objects.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="level">The level used for reporting.</param>
    /// <returns>Rules used for analyzing database objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="level"/> does not have a valid enum value.</exception>
    public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (!level.IsValid())
            throw new ArgumentException($"The {nameof(RuleLevel)} provided must be a valid enum.", nameof(level));

        return BuildRules(connection, level);
    }

    /// <summary>
    /// Retrieves the default set of rules used to analyze database objects, each at its own
    /// default reporting level.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <returns>Rules used for analyzing database objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public IEnumerable<IRule> GetRules(ISchematicConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return BuildRules(connection, level: null);
    }

    // One list serves both overloads: a null level means "leave each rule at its own
    // DefaultLevel", which is exactly what each rule's optional level parameter already does.
    private IEnumerable<IRule> BuildRules(ISchematicConnection connection, RuleLevel? level)
    {
        IEnumerable<IRule> rules =
        [
            new AutoIncrementColumnNotInKeyRule(level),
            new CandidateKeyMissingRule(level),
            new CascadeDeleteRule(level),
            new ColumnTypeMismatchAcrossTablesRule(level),
            new ColumnWithNullDefaultValueRule(level),
            new ColumnWithNumericSuffixRule(level),
            new DisabledObjectsRule(level),
            new EmptyRoutineDefinitionRule(level),
            new ForeignKeyColumnCollationMismatchRule(level),
            new ForeignKeyColumnTypeMismatchRule(level),
            new ForeignKeyIndexRule(level),
            new ForeignKeyIsPrimaryKeyRule(level),
            new ForeignKeyMissingRule(level),
            new ForeignKeyRelationshipCycleRule(level),
            new ForeignKeySelfReferenceRule(connection, level),
            new ForeignKeySetDefaultReferentialActionRule(level),
            new ForeignKeySetNullReferentialActionRule(level),
            new InconsistentColumnNamingConventionRule(level),
            new IndexOnLargeTextColumnRule(level),
            new InvalidSequenceConfigurationRule(level),
            new InvalidViewDefinitionRule(connection, level),
            new LikelyMisspelledNameRule(level),
            new NearDuplicateColumnNameRule(level),
            new NoIndexesPresentOnTableRule(level),
            new NoNonNullableColumnsPresentRule(level),
            new NoRowsPresentOnTableRule(connection, level, TableStatistics),
            new NoSurrogatePrimaryKeyRule(level),
            new NoValueForNullableColumnRule(connection, level, TableStatistics),
            new NullableBooleanColumnRule(level),
            new OnlyOneColumnPresentRule(level),
            new OrphanedTableRule(level),
            new PrimaryKeyColumnNotFirstColumnRule(level),
            new PrimaryKeyNotIntegerRule(level),
            new RedundantIndexesRule(level),
            new ReservedKeywordNameRule(connection.Dialect, level),
            new SelectStarInViewDefinitionRule(level),
            new TooManyColumnsRule(level),
            new TooManyIndexColumnsRule(level),
            new TriggerWithNoEnabledEventsRule(level),
            new UniqueIndexWithNullableColumnsRule(level),
            new UnvalidatedConstraintsRule(level),
            new WhitespaceNameRule(level),
        ];

        // Filtering the full list, rather than building a second one, keeps the remaining rules in
        // the same order either way, and that order is the order results are written in.
        return QueryDatabase
            ? rules
            : rules.Where(static rule => !QueriesDatabase(rule)).ToList();
    }

    // The rules that run their own queries against the database, rather than working only from
    // the schema objects they are given.
    private static bool QueriesDatabase(IRule rule) => rule
        is ForeignKeySelfReferenceRule
        or InvalidViewDefinitionRule
        or NoRowsPresentOnTableRule
        or NoValueForNullableColumnRule;
}