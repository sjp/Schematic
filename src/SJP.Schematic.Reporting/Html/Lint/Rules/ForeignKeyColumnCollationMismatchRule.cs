using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyColumnCollationMismatchRule : Schematic.Lint.Rules.ForeignKeyColumnCollationMismatchRule
{
    public ForeignKeyColumnCollationMismatchRule(RuleLevel level)
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
            .Append(" contains columns with mismatching collations. These should match to avoid implicit conversions and to ensure that joins and comparisons behave consistently.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
