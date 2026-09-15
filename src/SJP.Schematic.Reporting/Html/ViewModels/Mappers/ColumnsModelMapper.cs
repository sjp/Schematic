using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ColumnsModelMapper
{
    /// <summary>
    /// Maps a table's columns to rows of the columns summary list.
    /// </summary>
    /// <param name="table">The table whose columns are listed.</param>
    /// <param name="userDefinedTypeTargets">Resolves a column's declared type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public IEnumerable<Columns.ColumnSummary> Map(IRelationalDatabaseTable table, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

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
                userDefinedTypeTargets.GetTypeUrl(column.Type),
                column.IsNullable,
                column.DefaultValue,
                keyColumns.PrimaryKeyColumns.Contains(columnName),
                keyColumns.UniqueKeyColumns.Contains(columnName),
                keyColumns.ForeignKeyColumns.Contains(columnName)
            );
        }).ToList();
    }

    /// <summary>
    /// Maps a view's columns to rows of the columns summary list.
    /// </summary>
    /// <param name="view">The view whose columns are listed.</param>
    /// <param name="userDefinedTypeTargets">Resolves a column's declared type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="view"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public IEnumerable<Columns.ColumnSummary> Map(IDatabaseView view, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

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
                userDefinedTypeTargets.GetTypeUrl(c.Type),
                c.IsNullable,
                Option<string>.None,
                false,
                false,
                false
            )
        ).ToList();
    }
}
