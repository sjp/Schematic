using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules
{
    public class ForeignKeyRelationshipCycleRule : Rule, ITableRule
    {
        public ForeignKeyRelationshipCycleRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var cycleDetector = new CycleDetector();
            var cycles = cycleDetector.GetCyclePaths(tables.ToList());

            return cycles.Select(BuildMessage).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
        }

        protected virtual IRuleMessage BuildMessage(IReadOnlyCollection<Identifier> cyclePath)
        {
            if (cyclePath == null)
                throw new ArgumentNullException(nameof(cyclePath));

            var tableNames = cyclePath
                .Select(name => Identifier.CreateQualifiedIdentifier(name.Schema, name.LocalName).ToString())
                .Join(" -> ");
            var message = "Cycle found for the following path: " + tableNames;

            return new RuleMessage(RuleTitle, Level, message);
        }

        protected static string RuleTitle { get; } = "Foreign key relationships contain a cycle.";
    }
}
