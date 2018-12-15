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
    public class RedundantIndexesRule : Rule
    {
        public RedundantIndexesRule(RuleLevel level)
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
            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            return tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var indexes = table.Indexes;
            foreach (var index in indexes)
            {
                var indexColumnList = index.Columns
                    .SelectMany(c => c.DependentColumns)
                    .Select(c => c.Name);

                var otherIndexes = indexes.Where(i => i.Name != index.Name);
                foreach (var otherIndex in otherIndexes)
                {
                    var otherIndexColumnList = otherIndex.Columns
                        .SelectMany(c => c.DependentColumns)
                        .Select(c => c.Name);

                    var isPrefix = IsPrefixOf(indexColumnList, otherIndexColumnList);
                    if (isPrefix)
                    {
                        var redundantIndexColumns = index.Columns
                            .SelectMany(c => c.DependentColumns)
                            .Select(c => c.Name.LocalName);
                        var otherIndexColumns = otherIndex.Columns
                            .SelectMany(c => c.DependentColumns)
                            .Select(c => c.Name.LocalName);

                        var message = BuildMessage(table.Name, index.Name.LocalName, redundantIndexColumns, otherIndex.Name.LocalName, otherIndexColumns);
                        result.Add(message);
                    }
                }
            }

            return result;
        }

        protected static bool IsPrefixOf<T>(IEnumerable<T> prefixSet, IEnumerable<T> superSet)
        {
            if (prefixSet == null)
                throw new ArgumentNullException(nameof(prefixSet));
            if (superSet == null)
                throw new ArgumentNullException(nameof(superSet));

            var prefixSetList = prefixSet.ToList();
            if (prefixSetList.Empty())
                throw new ArgumentException("The given prefix set contained no values.", nameof(prefixSet));

            var superSetList = superSet.ToList();
            if (superSetList.Empty())
                throw new ArgumentException("The given super set contained no values.", nameof(superSet));

            if (prefixSetList.Count > superSetList.Count)
                return false;

            if (superSetList.Count > prefixSetList.Count)
                superSetList = superSetList.Take(prefixSetList.Count).ToList();

            return prefixSetList.SequenceEqual(superSetList);
        }

        protected virtual IRuleMessage BuildMessage(Identifier tableName, string indexName, IEnumerable<string> redundantIndexColumnNames, string otherIndexName, IEnumerable<string> otherIndexColumnNames)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(indexName));
            if (redundantIndexColumnNames == null || redundantIndexColumnNames.Empty())
                throw new ArgumentNullException(nameof(redundantIndexColumnNames));
            if (otherIndexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(otherIndexName));
            if (otherIndexColumnNames == null || otherIndexColumnNames.Empty())
                throw new ArgumentNullException(nameof(otherIndexColumnNames));

            var builder = StringBuilderCache.Acquire();
            builder.Append("The table ")
                .Append(tableName)
                .Append(" has an index '")
                .Append(indexName)
                .Append("' which may be redundant, as its column set (")
                .Append(redundantIndexColumnNames.Join(", "))
                .Append(") is the prefix of another index '")
                .Append(otherIndexName)
                .Append("' (")
                .Append(otherIndexColumnNames.Join(", "))
                .Append(").");

            var messageText = builder.GetStringAndRelease();
            return new RuleMessage(RuleTitle, Level, messageText);
        }

        protected static string RuleTitle { get; } = "Redundant indexes on a table.";
    }
}
