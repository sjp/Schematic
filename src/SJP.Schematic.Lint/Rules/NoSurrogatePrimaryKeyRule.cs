﻿using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules
{
    public class NoSurrogatePrimaryKeyRule : Rule
    {
        public NoSurrogatePrimaryKeyRule(RuleLevel level)
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

            return table.PrimaryKey
                .Match(
                    Some: pk =>
                    {
                        return pk.Columns.Count == 1
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
