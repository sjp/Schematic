using System;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class RedundantIndexesRule : Schematic.Lint.Rules.RedundantIndexesRule
{
    public RedundantIndexesRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, IDatabaseIndex redundantIndex, IDatabaseIndex otherIndex)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(redundantIndex);
        ArgumentNullException.ThrowIfNull(otherIndex);

        var redundantIndexColumnNames = redundantIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => $"'{c.Name.LocalName}'")
            .ToList();
        var redundantIncludedColumnNames = redundantIndex.IncludedColumns
            .Select(c => $"'{c.Name.LocalName}'")
            .ToList();
        var otherIndexColumnNames = otherIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => $"'{c.Name.LocalName}'")
            .ToList();
        var otherIncludedColumnNames = otherIndex.IncludedColumns
            .Select(c => $"'{c.Name.LocalName}'")
            .ToList();

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName.ToVisibleName())
            .Append(" has an index '")
            .Append(redundantIndex.Name.LocalName)
            .Append("' which may be redundant, as its column set (")
            .AppendJoin(", ", redundantIndexColumnNames)
            .Append(')');

        if (redundantIndex.IncludedColumns.Count > 0)
        {
            builder.Append(" INCLUDE (")
                .AppendJoin(", ", redundantIncludedColumnNames)
                .Append(')');
        }

        builder
            .Append(" is the prefix or subset of another index '")
            .Append(otherIndex.Name.LocalName)
            .Append("' (")
            .AppendJoin(", ", otherIndexColumnNames)
            .Append(')');

        if (otherIndex.IncludedColumns.Count > 0)
        {
            builder.Append(" INCLUDE (")
                .AppendJoin(", ", otherIncludedColumnNames)
                .Append(')');
        }

        builder.Append('.');

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
