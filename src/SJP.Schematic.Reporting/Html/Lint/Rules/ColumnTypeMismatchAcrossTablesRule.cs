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

    protected override IRuleMessage BuildMessage(string columnName, IReadOnlyCollection<Identifier> tableNames)
    {
        ArgumentNullException.ThrowIfNull(columnName);
        ArgumentNullException.ThrowIfNull(tableNames);

        var builder = StringBuilderCache.Acquire();
        builder.Append("The column '")
            .Append(columnName)
            .Append("' is declared with differing types across the following tables: ")
            .AppendJoin(", ", tableNames.Select(static t => t.ToVisibleName()))
            .Append(". Consider using a consistent type to avoid implicit conversions and join errors.");

        var messageText = builder.GetStringAndRelease();

        // Deliberately reported without an owning object: the finding is about a column name
        // shared by several unrelated tables, so attributing it to any one of them would send
        // the reader to a table that is no more at fault than the others.
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
