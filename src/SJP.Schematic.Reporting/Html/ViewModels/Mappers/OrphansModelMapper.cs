﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class OrphansModelMapper
{
    public Orphans.Table Map(IRelationalDatabaseTable table, ulong rowCount)
    {
        ArgumentNullException.ThrowIfNull(table);

        var columns = table.Columns;
        return new Orphans.Table(table.Name, columns.UCount(), rowCount);
    }
}