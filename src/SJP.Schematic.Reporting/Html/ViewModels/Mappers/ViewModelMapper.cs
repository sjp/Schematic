using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ViewModelMapper
{
    public View Map(IDatabaseView view, ReferencedObjectTargets referencedObjectTargets)
    {
        if (view == null)
            throw new ArgumentNullException(nameof(view));
        if (referencedObjectTargets == null)
            throw new ArgumentNullException(nameof(referencedObjectTargets));

        const string rootPath = "../";
        var links = referencedObjectTargets.GetReferencedObjectLinks(rootPath, view.Name, view.Definition);

        var viewColumns = view.Columns.ToList();
        var columns = viewColumns.Select(static (vc, i) =>
            new View.Column(
                vc.Name?.LocalName ?? string.Empty,
                i + 1,
                vc.IsNullable,
                vc.Type.Definition,
                vc.DefaultValue
            )).ToList();

        return new View(
            view.Name,
            rootPath,
            view.Definition,
            columns,
            links
        );
    }
}
