using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class NoSurrogatePrimaryKeyRule : Rule, ITableRule
    {
        public NoSurrogatePrimaryKeyRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
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
