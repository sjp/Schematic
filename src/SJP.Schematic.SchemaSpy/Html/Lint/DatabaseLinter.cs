
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.SchemaSpy.Html.Lint.Rules;

namespace SJP.Schematic.SchemaSpy.Html.Lint
{
    internal class DatabaseLinter
    {
        public DatabaseLinter(IDbConnection connection, IRelationalDatabase database, RuleLevel level = RuleLevel.Warning)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));
            Level = level;
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected RuleLevel Level { get; }

        public IEnumerable<IRuleMessage> AnalyzeDatabase() =>
            Rules.SelectMany(rule => rule.AnalyseDatabase(Database)).ToList();

        protected IEnumerable<Schematic.Lint.Rule> Rules => new Schematic.Lint.Rule[]
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
