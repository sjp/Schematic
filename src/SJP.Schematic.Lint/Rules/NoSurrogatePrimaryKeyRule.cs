using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class NoSurrogatePrimaryKeyRule : Rule, ITableRule
    {
        public NoSurrogatePrimaryKeyRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToList();
        }

        public Task<IEnumerable<IRuleMessage>> AnalyseTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            var messages = AnalyseTables(tables);
            return Task.FromResult(messages);
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return table.PrimaryKey
                .Match(
                    Some: pk =>
                    {
                        if (pk.Columns.Count == 1)
                            return Array.Empty<IRuleMessage>();

                        var fkColumns = table.ParentKeys
                            .Select(fk => fk.ChildKey)
                            .SelectMany(fk => fk.Columns)
                            .Select(fkc => fkc.Name.LocalName)
                            .Distinct()
                            .ToList();

                        var areAllColumnsFks = pk.Columns.All(c => fkColumns.Contains(c.Name.LocalName));
                        return areAllColumnsFks
                            ? Array.Empty<IRuleMessage>()
                            : new[] { BuildMessage(table.Name) };
                    },
                    None: Array.Empty<IRuleMessage>
                );
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var messageText = $"The table { tableName } has a multi-column primary key. Consider introducing a surrogate primary key.";
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "No surrogate primary key present on table.";
    }
}
