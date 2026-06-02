using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class UniqueIndexWithNullableColumnsRule : Schematic.Lint.Rules.UniqueIndexWithNullableColumnsRule
{
    public UniqueIndexWithNullableColumnsRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, string? indexName, IEnumerable<string> columnNames)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnNames);
        if (columnNames.Empty())
            throw new ArgumentException("At least one column name is required.", nameof(columnNames));

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName.ToVisibleName())
            .Append(" has a unique index");

        if (!indexName.IsNullOrWhiteSpace())
        {
            builder.Append(" '")
                .Append(indexName)
                .Append('\'');
        }

        var pluralText = columnNames.Skip(1).Any()
            ? " which contains nullable columns: "
            : " which contains a nullable column: ";
        builder.Append(pluralText);

        builder.AppendJoin(", ", columnNames.Select(static columnName => $"'{columnName}'"));

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
