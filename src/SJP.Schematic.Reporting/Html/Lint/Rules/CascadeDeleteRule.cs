using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class CascadeDeleteRule : Schematic.Lint.Rules.CascadeDeleteRule
{
    public CascadeDeleteRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName, Identifier parentTableName)
    {
        ArgumentNullException.ThrowIfNull(childTableName);
        ArgumentNullException.ThrowIfNull(parentTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("A foreign key");
        foreignKeyName.IfSome(name =>
        {
            builder.Append(" '")
                .Append(name.LocalName)
                .Append('\'');
        });

        builder.Append(" from ")
            .Append(childTableName.ToVisibleName())
            .Append(" to ")
            .Append(parentTableName.ToVisibleName())
            .Append(" has a CASCADE delete action. Deleting a parent row will also delete the related child rows; ensure this is intended, as cascades can propagate and remove large amounts of data.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
