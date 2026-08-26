using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ColumnTypeMismatchAcrossTablesRule : Schematic.Lint.Rules.ColumnTypeMismatchAcrossTablesRule
{
    public ColumnTypeMismatchAcrossTablesRule(RuleLevel? level = null)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(string columnName, IReadOnlyCollection<(string TypeDefinition, IReadOnlyCollection<Identifier> TableNames)> typeGroups)
    {
        ArgumentNullException.ThrowIfNull(columnName);
        ArgumentNullException.ThrowIfNull(typeGroups);

        var builder = StringBuilderCache.Acquire();
        builder.Append("The column '")
            .Append(columnName)
            .Append("' is declared with differing types across tables: ");

        var firstGroup = true;
        foreach (var (typeDefinition, tableNames) in typeGroups)
        {
            if (!firstGroup)
                builder.Append("; ");
            firstGroup = false;

            builder.Append(typeDefinition)
                .Append(" in ")
                .AppendJoin(", ", tableNames.Select(static t => t.ToVisibleName()));
        }

        builder.Append(". Consider using a consistent type to avoid implicit conversions and join errors.");

        var messageText = builder.GetStringAndRelease();

        // Deliberately reported without an owning object: the finding is about a column name
        // shared by several unrelated tables, so attributing it to any one of them would send
        // the reader to a table that is no more at fault than the others.
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
