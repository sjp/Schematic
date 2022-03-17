using System;
using System.Linq;
using System.Web;
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
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (primaryKey == null)
            throw new ArgumentNullException(nameof(primaryKey));
        if (foreignKey == null)
            throw new ArgumentNullException(nameof(foreignKey));

        var tableUrl = UrlRouter.GetTableUrl(tableName);
        var tableLink = $"<a href=\"{ tableUrl }\">{ HttpUtility.HtmlEncode(tableName.ToVisibleName()) }</a>";

        var primaryKeyColumnNames = primaryKey.Columns
            .Select(c => Dialect.QuoteIdentifier(c.Name.LocalName))
            .Select(HttpUtility.HtmlEncode)
            .Select(c => "<code>" + c + "</code>");
        var pkNameSuffix = primaryKey.Name.Match(
            pkName => $"<code>{ HttpUtility.HtmlEncode(Dialect.QuoteName(pkName)) }</code> ",
            () => string.Empty
        );
        var primaryKeyMessage = $"primary key { pkNameSuffix }({ primaryKeyColumnNames.Join(", ") })";

        var foreignKeyColumnNames = foreignKey.Columns
            .Select(c => Dialect.QuoteIdentifier(c.Name.LocalName))
            .Select(HttpUtility.HtmlEncode)
            .Select(c => "<code>" + c + "</code>");
        var fkNameSuffix = foreignKey.Name.Match(
            fkName => $"<code>{ HttpUtility.HtmlEncode(Dialect.QuoteName(fkName)) }</code>",
            () => string.Empty
        );
        var foreignKeyMessage = $"foreign key { fkNameSuffix }({ foreignKeyColumnNames.Join(", ") })";

        var messageText = $"The table { tableLink } contains a row where the { foreignKeyMessage } self-references the { primaryKeyMessage }. Consider removing the row by removing the foreign key first, then reintroducing after row removal.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }
}
