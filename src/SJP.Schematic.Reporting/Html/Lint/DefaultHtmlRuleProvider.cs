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
    /// Retrieves the default set of rules used to analyze database objects.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="level">The level used for reporting.</param>
    /// <returns>Rules used for analyzing database objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="level"/> does not have a valid enum value.</exception>
    public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (!level.IsValid())
            throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));

        return new IRule[]
        {
            new CandidateKeyMissingRule(level),
            new ColumnWithNullDefaultValueRule(level),
            new ColumnWithNumericSuffix(level),
            new DisabledObjectsRule(level),
            new ForeignKeyColumnTypeMismatchRule(level),
            new ForeignKeyIndexRule(level),
            new ForeignKeyIsPrimaryKeyRule(level),
            new ForeignKeyMissingRule(level),
            new ForeignKeyRelationshipCycleRule(level),
            new ForeignKeySelfReferenceRule(connection, level),
            new InvalidViewDefinitionRule(connection, level),
            new NoIndexesPresentOnTableRule(level),
            new NoNonNullableColumnsPresentRule(level),
            new NoSurrogatePrimaryKeyRule(level),
            new NoValueForNullableColumnRule(connection, level),
            new OnlyOneColumnPresentRule(level),
            new OrphanedTableRule(level),
            new PrimaryKeyColumnNotFirstColumnRule(level),
            new PrimaryKeyNotIntegerRule(level),
            new RedundantIndexesRule(level),
            new ReservedKeywordNameRule(connection.Dialect, level),
            new TooManyColumnsRule(level),
            new UniqueIndexWithNullableColumnsRule(level),
            new WhitespaceNameRule(level)
        };
    }
}