using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyRelationshipCycleRule : Schematic.Lint.Rules.ForeignKeyRelationshipCycleRule
{
    public ForeignKeyRelationshipCycleRule(RuleLevel level)
        : base(level)
    {
    }

    protected override IRuleMessage BuildMessage(IReadOnlyCollection<Identifier> cyclePath)
    {
        ArgumentNullException.ThrowIfNull(cyclePath);

        var tableNames = cyclePath
            .Select(static tableName => tableName.ToVisibleName())
            .Join(" → ");
        var message = "Cycle found for the following path: " + tableNames;

        return new RuleMessage(RuleId, RuleTitle, Level, message);
    }
}
