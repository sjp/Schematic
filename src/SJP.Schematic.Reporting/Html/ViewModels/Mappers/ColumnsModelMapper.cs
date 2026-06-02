using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ColumnsModelMapper
{
    public IEnumerable<Columns.ColumnSummary> Map(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var primaryKey = table.PrimaryKey;
        var uniqueKeys = table.UniqueKeys.ToList();
        var parentKeys = table.ParentKeys.ToList();

        var columns = table.Columns.ToList();
        var tableUrl = UrlRouter.GetTableUrl(table.Name);

        return columns.Select((column, i) =>
        {
            var isPrimaryKeyColumn = primaryKey.Match(pk => pk.Columns.Any(c => string.Equals(c.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal)), static () => false);
            var isUniqueKeyColumn = uniqueKeys.Exists(uk => uk.Columns.Any(ukc => string.Equals(ukc.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal)));
            var isForeignKeyColumn = parentKeys.Exists(fk => fk.ChildKey.Columns.Any(fkc => string.Equals(fkc.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal)));

            return new Columns.ColumnSummary(
                table.Name,
                Columns.ParentObjectType.Table,
                tableUrl,
                i + 1,
                column.Name.LocalName,
                column.Type.Definition,
                column.IsNullable,
                column.DefaultValue,
                isPrimaryKeyColumn,
                isUniqueKeyColumn,
                isForeignKeyColumn
            );
        }).ToList();
    }

    public IEnumerable<Columns.ColumnSummary> Map(IDatabaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var columns = view.Columns.ToList();
        var viewUrl = UrlRouter.GetViewUrl(view.Name);

        return columns.Select((c, i) =>
            new Columns.ColumnSummary(
                view.Name,
                Columns.ParentObjectType.View,
                viewUrl,
                i + 1,
                c.Name.LocalName,
                c.Type.Definition,
                c.IsNullable,
                Option<string>.None,
                false,
                false,
                false
            )
        ).ToList();
    }
}
