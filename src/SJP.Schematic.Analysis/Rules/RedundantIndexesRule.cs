using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis.Rules
{
    public class RedundantIndexesRule : Rule
    {
        public RedundantIndexesRule(RuleLevel level)
            : base(RuleTitle, level)
        {
        }

        public override IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return database.Tables.SelectMany(AnalyseTable).ToList();
        }

        protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var result = new List<IRuleMessage>();

            var indexes = table.Indexes.ToList();
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
                        var messageText = BuildMessage(table.Name, index, otherIndex);
                        var ruleMessage = new RuleMessage(Title, Level, messageText);
                        result.Add(ruleMessage);
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
            if (prefixSetList.Count == 0)
                throw new ArgumentException("The given prefix set contained no values.", nameof(prefixSet));

            var superSetList = superSet.ToList();
            if (superSetList.Count == 0)
                throw new ArgumentException("The given super set contained no values.", nameof(superSet));

            if (prefixSetList.Count > superSetList.Count)
                return false;

            if (superSetList.Count > prefixSetList.Count)
                superSetList = superSetList.Take(prefixSetList.Count).ToList();

            return prefixSetList.SequenceEqual(superSetList);
        }

        protected static string BuildMessage(Identifier tableName, IDatabaseTableIndex redundantIndex, IDatabaseTableIndex otherIndex)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (redundantIndex == null)
                throw new ArgumentNullException(nameof(redundantIndex));
            if (otherIndex == null)
                throw new ArgumentNullException(nameof(otherIndex));

            var redundantIndexColumns = redundantIndex.Columns
                .SelectMany(c => c.DependentColumns)
                .Select(c => c.Name.LocalName)
                .Join(", ");

            var builder = new StringBuilder("The table ")
                .Append(tableName.ToString())
                .Append(" has an index '")
                .Append(redundantIndex.Name.LocalName)
                .Append("' which may be redundant, as its column set (")
                .Append(redundantIndexColumns)
                .Append(") is the prefix of another index '")
                .Append(otherIndex.Name.LocalName)
                .Append("'.");

            return builder.ToString();
        }

        private const string RuleTitle = "Indexes missing on foreign key.";
    }
}
