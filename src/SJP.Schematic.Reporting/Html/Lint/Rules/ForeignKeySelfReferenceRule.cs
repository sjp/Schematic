using System;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class ForeignKeySelfReferenceRule : Schematic.Lint.Rules.ForeignKeySelfReferenceRule
{
    public ForeignKeySelfReferenceRule(ISchematicConnection connection, RuleLevel? level = null)
        : base(connection, level)
    {
    }

    protected override IRuleMessage BuildMessage(Identifier tableName, IDatabaseKey targetKey, IDatabaseKey foreignKey)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(targetKey);
        ArgumentNullException.ThrowIfNull(foreignKey);

        var targetKeyColumnNames = targetKey.Columns
            .Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var targetKeyNameSuffix = targetKey.Name.Match(
            targetKeyName => $"{Dialect.QuoteName(targetKeyName)} ",
            () => string.Empty
        );
        var targetKeyMessage = $"{GetKeyTypeDescription(targetKey.KeyType)} {targetKeyNameSuffix}({targetKeyColumnNames.Join(", ")})";

        var foreignKeyColumnNames = foreignKey.Columns
            .Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var fkNameSuffix = foreignKey.Name.Match(
            fkName => $"{Dialect.QuoteName(fkName)} ",
            () => string.Empty
        );
        var foreignKeyMessage = $"foreign key {fkNameSuffix}({foreignKeyColumnNames.Join(", ")})";

        var messageText = $"The table {tableName.ToVisibleName()} contains a row where the {foreignKeyMessage} self-references the {targetKeyMessage}. Consider removing the row by removing the foreign key first, then reintroducing after row removal.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }
}
