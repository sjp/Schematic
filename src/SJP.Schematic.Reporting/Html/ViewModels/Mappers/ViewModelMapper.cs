using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ViewModelMapper
{
    public View Map(IDatabaseView view, ReferencedObjectTargets referencedObjectTargets)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(referencedObjectTargets);

        var referencedObjects = referencedObjectTargets.GetReferencedObjects(view.Name, view.Definition);

        var viewColumns = view.Columns.ToList();
        var columns = viewColumns.Select(static (vc, i) =>
            new View.ViewColumn(
                vc.Name?.LocalName ?? string.Empty,
                i + 1,
                vc.IsNullable,
                vc.Type.Definition,
                vc.DefaultValue
            )).ToList();

        return new View(
            view.Name,
            view.Definition,
            columns,
            referencedObjects
        );
    }
}
