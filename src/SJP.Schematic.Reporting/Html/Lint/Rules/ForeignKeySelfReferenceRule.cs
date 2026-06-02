using System;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeySelfReferenceRule : Schematic.Lint.Rules.ForeignKeySelfReferenceRule
{
    public ForeignKeySelfReferenceRule(ISchematicConnection connection, RuleLevel level)
        : base(connection, level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, IDatabaseKey primaryKey, IDatabaseKey foreignKey)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(primaryKey);
        ArgumentNullException.ThrowIfNull(foreignKey);

        var primaryKeyColumnNames = primaryKey.Columns
            .Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var pkNameSuffix = primaryKey.Name.Match(
            pkName => $"{Dialect.QuoteName(pkName)} ",
            () => string.Empty
        );
        var primaryKeyMessage = $"primary key {pkNameSuffix}({primaryKeyColumnNames.Join(", ")})";

        var foreignKeyColumnNames = foreignKey.Columns
            .Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var fkNameSuffix = foreignKey.Name.Match(
            fkName => $"{Dialect.QuoteName(fkName)} ",
            () => string.Empty
        );
        var foreignKeyMessage = $"foreign key {fkNameSuffix}({foreignKeyColumnNames.Join(", ")})";

        var messageText = $"The table {tableName.ToVisibleName()} contains a row where the {foreignKeyMessage} self-references the {primaryKeyMessage}. Consider removing the row by removing the foreign key first, then reintroducing after row removal.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
