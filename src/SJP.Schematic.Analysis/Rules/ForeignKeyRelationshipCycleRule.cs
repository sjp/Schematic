using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Analysis.Rules
{
    public class ForeignKeyRelationshipCycleRule : Rule
    {
        protected ForeignKeyRelationshipCycleRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var graph = new Multigraph<Identifier, IDatabaseRelationalKey>();

            var foreignKeys = database.Tables.SelectMany(t => t.ParentKeys).ToList();
            foreach (var foreignKey in foreignKeys)
                graph.AddEdge(foreignKey.ChildKey.Table.Name, foreignKey.ParentKey.Table.Name, foreignKey);

            try
            {
                var cycle = graph.TopologicalSort();
                return Enumerable.Empty<IRuleMessage>();
            }
            catch (Exception ex)
            {
                var ruleMessage = new RuleMessage(RuleTitle, Level, ex.Message);
                return new[] { ruleMessage };
            }
        }

        private const string RuleTitle = "Foreign key relationships contain a cycle.";
    }
}
