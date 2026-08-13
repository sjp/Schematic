using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class OrphansModelMapper
{
    public Orphans.OrphanTable Map(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var columns = table.Columns;
        return new Orphans.OrphanTable(table.Name, columns.UCount());
    }
}