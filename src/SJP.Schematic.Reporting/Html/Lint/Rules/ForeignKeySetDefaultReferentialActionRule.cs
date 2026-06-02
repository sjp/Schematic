using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeySetDefaultReferentialActionRule : Schematic.Lint.Rules.ForeignKeySetDefaultReferentialActionRule
{
    public ForeignKeySetDefaultReferentialActionRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier childTableName)
    {
        ArgumentNullException.ThrowIfNull(childTableName);

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(childTableName.ToVisibleName())
            .Append(" has a foreign key");
        foreignKeyName.IfSome(name =>
        {
            builder.Append(" '")
                .Append(name.LocalName)
                .Append('\'');
        });

        builder.Append(" with a SET DEFAULT referential action, but one or more of its columns have no default value. The action can never succeed and will cause referential operations to fail.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
