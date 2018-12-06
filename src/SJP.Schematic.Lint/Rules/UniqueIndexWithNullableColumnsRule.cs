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
    public class UniqueIndexWithNullableColumnsRule : Rule
    {
        public UniqueIndexWithNullableColumnsRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        public override Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsync(IRelationalDatabase database, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return AnalyseDatabaseAsyncCore(database, cancellationToken);
        }

        private async Task<IEnumerable<IRuleMessage>> AnalyseDatabaseAsyncCore(IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var tables = await database.TablesAsync(cancellationToken).ConfigureAwait(false);
            return tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var uniqueIndexes = table.Indexes.Where(i => i.IsUnique).ToList();
            if (uniqueIndexes.Empty())
                return Array.Empty<IRuleMessage>();

            var result = new List<IRuleMessage>();

            foreach (var index in uniqueIndexes)
            {
                var nullableColumns = index.Columns
                    .SelectMany(c => c.DependentColumns)
                    .Where(c => c.IsNullable)
                    .ToList();

                if (nullableColumns.Empty())
                    continue;

                var columnNames = nullableColumns.Select(c => c.Name.LocalName).ToList();
                var message = BuildMessage(table.Name, index.Name?.LocalName, columnNames);
                result.Add(message);
            }

            return result;
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, string indexName, IEnumerable<string> columnNames)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columnNames == null || columnNames.Empty())
                throw new ArgumentNullException(nameof(columnNames));

            var builder = StringBuilderCache.Acquire();
            builder.Append("The table ")
                .Append(tableName.ToString())
                .Append(" has a unique index ");

            if (!indexName.IsNullOrWhiteSpace())
            {
                builder.Append("'")
                    .Append(indexName)
                    .Append("' ");
            }

            var pluralText = columnNames.Skip(1).Any()
                ? "which contains nullable columns: "
                : "which contains a nullable column: ";
            builder.Append(pluralText);

            var joinedColumnNames = columnNames.Join(", ");
            builder.Append(joinedColumnNames);

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Unique index contains nullable columns.";
    }
}
