using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Html.Lint;

/// <summary>
/// Constructs a rule provider that returns a default set of rules.
/// </summary>
/// <seealso cref="IRuleProvider" />
public sealed class DefaultHtmlRuleProvider : IRuleProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHtmlRuleProvider"/> class, whose rules query the
    /// database for anything the schema does not tell them.
    /// </summary>
    public DefaultHtmlRuleProvider()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHtmlRuleProvider"/> class.
    /// </summary>
    /// <param name="tableStatistics">The statistics the database records for its tables, given to the rules that can use them in place of a query. <see langword="null" /> when none are available.</param>
    public DefaultHtmlRuleProvider(ITableStatisticsProvider? tableStatistics)
    {
        TableStatistics = tableStatistics;
    }

    /// <summary>
    /// The statistics the database records for its tables, if any were supplied.
    /// </summary>
    /// <value>A table statistics provider.</value>
    private ITableStatisticsProvider? TableStatistics { get; }

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
        return
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
            new NoIndexesPresentOnTableRule(level),
            new NoNonNullableColumnsPresentRule(level),
            new NoRowsPresentOnTableRule(connection, level, TableStatistics),
            new NoSurrogatePrimaryKeyRule(level),
            new NoValueForNullableColumnRule(connection, level),
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
    }
}