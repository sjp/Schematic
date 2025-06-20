using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class RelationshipsModelMapper
{
    public RelationshipsModelMapper(IIdentifierDefaults identifierDefaults)
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    private IIdentifierDefaults IdentifierDefaults { get; }

    public Relationships Map(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(rowCounts);

        var dotFormatter = new DotFormatter(IdentifierDefaults);

        var compactOptions = new DotRenderOptions { IsReducedColumnSet = true };
        var compactDot = dotFormatter.RenderTables(tables, rowCounts, compactOptions);

        var largeOptions = new DotRenderOptions { IsReducedColumnSet = false };
        var largeDot = dotFormatter.RenderTables(tables, rowCounts, largeOptions);

        var diagrams = new[]
        {
            new Relationships.Diagram("Compact", compactDot, true),
            new Relationships.Diagram("Large", largeDot, false)
        };

        return new Relationships(diagrams);
    }
}