using System;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class IndexOnLargeTextColumnRule : Schematic.Lint.Rules.IndexOnLargeTextColumnRule
{
    public IndexOnLargeTextColumnRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, Identifier indexName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(indexName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The table {tableName.ToVisibleName()} has an index '{indexName.LocalName}' defined over the large text or binary column '{columnName}'. Indexing such columns is often expensive and ineffective; consider indexing a derived or truncated value instead.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
