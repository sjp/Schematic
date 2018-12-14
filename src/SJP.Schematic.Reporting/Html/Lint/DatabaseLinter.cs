using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Html.Lint
{
    internal sealed class DatabaseLinter
    {
        public DatabaseLinter(IDbConnection connection, IRelationalDatabase database, RuleLevel level = RuleLevel.Warning)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));
            Level = level;
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private RuleLevel Level { get; }

        public async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(CancellationToken cancellationToken)
        {
            var result = new List<IRuleMessage>();

            foreach (var rule in Rules)
            {
                var messages = await rule.AnalyseDatabaseAsync(Database, cancellationToken).ConfigureAwait(false);
                result.AddRange(messages);
            }

            return result;
        }

        private IEnumerable<IRule> Rules => new IRule[]
        {
            new CandidateKeyMissingRule(Level),
            new ColumnWithNullDefaultValueRule(Level),
            new DisabledObjectsRule(Level),
            new ForeignKeyColumnTypeMismatchRule(Level),
            new ForeignKeyIndexRule(Level),
            new ForeignKeyIsPrimaryKeyRule(Level),
            new ForeignKeyRelationshipCycleRule(Level),
            new InvalidViewDefinitionRule(Connection, Level),
            new NoNonNullableColumnsPresentRule(Level),
            new NoSurrogatePrimaryKeyRule(Level),
            new NoValueForNullableColumnRule(Connection, Level),
            new OnlyOneColumnPresentRule(Level),
            new OrphanedTableRule(Level),
            new PrimaryKeyColumnNotFirstColumnRule(Level),
            new PrimaryKeyNotIntegerRule(Level),
            new RedundantIndexesRule(Level),
            new ReservedKeywordNameRule(Level),
            new TooManyColumnsRule(Level),
            new UniqueIndexWithNullableColumnsRule(Level),
            new WhitespaceNameRule(Level)
        };
    }
}
