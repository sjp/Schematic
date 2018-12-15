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
    public class ForeignKeyRelationshipCycleRule : Rule
    {
        public ForeignKeyRelationshipCycleRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var graph = new Multigraph<Identifier, IDatabaseRelationalKey>();
            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            graph.AddVertices(tables.Select(t => t.Name));

            var foreignKeys = tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk => fk.ChildTable != fk.ParentTable)
                .ToList();
            foreach (var foreignKey in foreignKeys)
                graph.AddEdge(foreignKey.ChildTable, foreignKey.ParentTable, foreignKey);

            try
            {
                graph.TopologicalSort();
                return Array.Empty<IRuleMessage>();
            }
            catch (Exception ex)
            {
                var message = BuildMessage(ex.Message);
                return new[] { message };
            }
        }

        protected virtual IRuleMessage BuildMessage(string exceptionMessage)
        {
            if (exceptionMessage.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(exceptionMessage));

            return new RuleMessage(RuleTitle, Level, exceptionMessage);
        }

        protected static string RuleTitle { get; } = "Foreign key relationships contain a cycle.";
    }
}
