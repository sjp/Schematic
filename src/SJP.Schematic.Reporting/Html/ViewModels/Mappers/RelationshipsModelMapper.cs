using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class RelationshipsModelMapper
{
    public Relationships Map(IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var graph = RelationshipGraphMapper.Map(tables);
        return new Relationships(graph);
    }
}
