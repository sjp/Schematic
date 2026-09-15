using System;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ViewModelMapper
{
    /// <summary>
    /// Maps a view to its detail payload.
    /// </summary>
    /// <param name="view">The view to map.</param>
    /// <param name="referencedObjectTargets">Resolves the objects the view's definition references to their pages.</param>
    /// <param name="userDefinedTypeTargets">Resolves a column's declared type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException">Any argument is <see langword="null" />.</exception>
    public View Map(IDatabaseView view, ReferencedObjectTargets referencedObjectTargets, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(referencedObjectTargets);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        var referencedObjects = referencedObjectTargets.GetReferencedObjects(view.Name, view.Definition);

        var viewColumns = view.Columns.ToList();
        var columns = viewColumns.Select((vc, i) =>
            new View.ViewColumn(
                vc.Name?.LocalName ?? string.Empty,
                i + 1,
                vc.IsNullable,
                vc.Type.Definition,
                userDefinedTypeTargets.GetTypeUrl(vc.Type),
                vc.DefaultValue
            )).ToList();

        var indexes = view.Indexes.Select(static index =>
            new Table.Index(
                index.Name?.LocalName,
                index.IsUnique,
                index.Columns.Select(static c => c.Expression).ToList(),
                index.Columns.Select(static c => c.Order).ToList(),
                index.IncludedColumns.Select(static c => c.Name.LocalName).ToList(),
                index.IndexType,
                index.FilterDefinition,
                index.IsEnabled,
                index.IsValid,
                index.IsVisible
            )).ToList();

        var triggers = view.Triggers.Select(static tr =>
            new Table.Trigger(
                tr.Name,
                tr.Definition,
                tr.QueryTiming,
                tr.TriggerEvent,
                tr.Granularity,
                tr.Condition,
                tr.UpdateColumns
            )).ToList();

        var materializedView = view as IDatabaseMaterializedView;

        return new View(
            view.Name,
            view.Definition,
            columns,
            referencedObjects,
            indexes,
            triggers,
            view.CheckOption,
            view.IsUpdatable,
            view.IsMaterialized,
            materializedView?.RefreshMode ?? MaterializedViewRefreshMode.Unknown,
            materializedView?.RefreshMethod ?? Option<string>.None,
            materializedView?.IsPopulated ?? false
        );
    }
}
