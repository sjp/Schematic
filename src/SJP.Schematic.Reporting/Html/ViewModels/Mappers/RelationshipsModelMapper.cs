using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class RelationshipsModelMapper
{
    public Relationships Map(IReadOnlyCollection<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(rowCounts);

        var graph = RelationshipGraphMapper.Map(tables, rowCounts);
        return new Relationships(graph);
    }
}
