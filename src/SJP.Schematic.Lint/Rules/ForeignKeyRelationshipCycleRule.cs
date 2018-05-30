using System;
using System.Collections.Generic;
using System.Linq;
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

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var graph = new Multigraph<Identifier, IDatabaseRelationalKey>();
            graph.AddVertices(database.Tables.Select(t => t.Name));

            var foreignKeys = database.Tables
                .SelectMany(t => t.ParentKeys)
                .Where(fk => fk.ChildKey.Table.Name != fk.ParentKey.Table.Name)
                .ToList();
            foreach (var foreignKey in foreignKeys)
                graph.AddEdge(foreignKey.ChildKey.Table.Name, foreignKey.ParentKey.Table.Name, foreignKey);

            try
            {
                graph.TopologicalSort();
                return Enumerable.Empty<IRuleMessage>();
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
