using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ViewModelMapper
    {
        public View Map(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var viewColumns = view.Columns.ToList();

            var columns = viewColumns.Select((vc, i) =>
                new View.Column(
                    vc.Name?.LocalName ?? string.Empty,
                    i + 1,
                    vc.IsNullable,
                    vc.Type.Definition,
                    vc.DefaultValue
                )).ToList();

            return new View(
                view.Name,
                "../",
                view.Definition,
                columns
            );
        }
    }
}
