using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeyRelationshipCycleRule : Schematic.Lint.Rules.ForeignKeyRelationshipCycleRule
{
    public ForeignKeyRelationshipCycleRule(RuleLevel? level = null)
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

        // A cycle belongs to every table on the path rather than to one of them. Anchor it to the
        // table the path starts at so the report can still link it somewhere useful.
        var anchorTable = cyclePath.Count > 0
            ? Option<Identifier>.Some(cyclePath.First())
            : Option<Identifier>.None;

        return new RuleMessage(RuleId, RuleTitle, Level, message, anchorTable);
    }
}
