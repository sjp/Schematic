using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class SynonymTargets
{
    public SynonymTargets(
        IEnumerable<Identifier> tableNames,
        IEnumerable<Identifier> viewNames,
        IEnumerable<Identifier> sequenceNames,
        IEnumerable<Identifier> synonymNames,
        IEnumerable<Identifier> routineNames
    )
    {
        ArgumentNullException.ThrowIfNull(tableNames);
        ArgumentNullException.ThrowIfNull(viewNames);
        ArgumentNullException.ThrowIfNull(sequenceNames);
        ArgumentNullException.ThrowIfNull(synonymNames);
        ArgumentNullException.ThrowIfNull(routineNames);

        // Match target names case-insensitively, consistent with ReferencedObjectTargets — so a
        // synonym whose target differs only in casing still resolves to its object's route.
        TableNames = new HashSet<Identifier>(tableNames, IdentifierComparer.OrdinalIgnoreCase);
        ViewNames = new HashSet<Identifier>(viewNames, IdentifierComparer.OrdinalIgnoreCase);
        SequenceNames = new HashSet<Identifier>(sequenceNames, IdentifierComparer.OrdinalIgnoreCase);
        SynonymNames = new HashSet<Identifier>(synonymNames, IdentifierComparer.OrdinalIgnoreCase);
        RoutineNames = new HashSet<Identifier>(routineNames, IdentifierComparer.OrdinalIgnoreCase);
    }

    public IReadOnlySet<Identifier> TableNames { get; }

    public IReadOnlySet<Identifier> ViewNames { get; }

    public IReadOnlySet<Identifier> SequenceNames { get; }

    public IReadOnlySet<Identifier> SynonymNames { get; }

    public IReadOnlySet<Identifier> RoutineNames { get; }
}