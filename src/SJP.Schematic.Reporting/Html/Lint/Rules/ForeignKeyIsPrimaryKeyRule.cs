using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyIsPrimaryKeyRule : Schematic.Lint.Rules.ForeignKeyIsPrimaryKeyRule
{
    public ForeignKeyIsPrimaryKeyRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName)
    {
        ArgumentNullException.ThrowIfNull(childTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("A foreign key");

        foreignKeyName.IfSome(fkName =>
        {
            builder.Append(" '")
                .Append(fkName.LocalName)
                .Append('\'');
        });

        builder.Append(" on ")
            .Append(childTableName.ToVisibleName())
            .Append(" contains the same column set as the target key.");

        var message = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, message);
    }
}
