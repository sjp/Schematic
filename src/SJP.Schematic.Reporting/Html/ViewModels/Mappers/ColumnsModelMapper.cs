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

        var keyColumns = table.GetKeyColumns();

        var columns = table.Columns.ToList();
        var tableUrl = UrlRouter.GetTableUrl(table.Name);

        return columns.Select((column, i) =>
        {
            var columnName = column.Name.LocalName;

            return new Columns.ColumnSummary(
                table.Name,
                Columns.ParentObjectType.Table,
                tableUrl,
                i + 1,
                columnName,
                column.Type.Definition,
                column.IsNullable,
                column.DefaultValue,
                keyColumns.PrimaryKeyColumns.Contains(columnName),
                keyColumns.UniqueKeyColumns.Contains(columnName),
                keyColumns.ForeignKeyColumns.Contains(columnName)
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
