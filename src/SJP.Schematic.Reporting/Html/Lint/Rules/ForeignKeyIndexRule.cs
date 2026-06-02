using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyIndexRule : Schematic.Lint.Rules.ForeignKeyIndexRule
{
    public ForeignKeyIndexRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier tableName, IEnumerable<string> columnNames)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        if (columnNames.NullOrEmpty())
            throw new ArgumentNullException(nameof(columnNames));

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName.ToVisibleName())
            .Append(" has a foreign key");

        foreignKeyName.IfSome(fkName =>
        {
            builder.Append(" '")
                .Append(fkName.LocalName)
                .Append('\'');
        });

        builder.Append(" which is missing an index on the column");

        // plural check
        if (columnNames.Skip(1).Any())
            builder.Append('s');

        builder.Append(' ')
            .AppendJoin(", ", columnNames.Select(static columnName => $"'{columnName}'"));

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
