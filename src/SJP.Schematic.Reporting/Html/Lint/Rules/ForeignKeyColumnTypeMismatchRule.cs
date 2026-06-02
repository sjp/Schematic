using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyColumnTypeMismatchRule : Schematic.Lint.Rules.ForeignKeyColumnTypeMismatchRule
{
    public ForeignKeyColumnTypeMismatchRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName, Identifier parentTableName)
    {
        ArgumentNullException.ThrowIfNull(childTableName);
        ArgumentNullException.ThrowIfNull(parentTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("A foreign key");

        foreignKeyName.IfSome(fkName =>
        {
            builder.Append(" '")
                .Append(fkName.LocalName)
                .Append('\'');
        });

        builder.Append(" from ")
            .Append(childTableName.ToVisibleName())
            .Append(" to ")
            .Append(parentTableName.ToVisibleName())
            .Append(" contains mismatching column types. These should be the same in order to ensure that foreign keys can always hold the same information as the target key.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
