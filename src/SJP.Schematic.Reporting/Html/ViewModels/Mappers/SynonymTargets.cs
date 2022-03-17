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
        TableNames = tableNames ?? throw new ArgumentNullException(nameof(tableNames));
        ViewNames = viewNames ?? throw new ArgumentNullException(nameof(viewNames));
        SequenceNames = sequenceNames ?? throw new ArgumentNullException(nameof(sequenceNames));
        SynonymNames = synonymNames ?? throw new ArgumentNullException(nameof(synonymNames));
        RoutineNames = routineNames ?? throw new ArgumentNullException(nameof(routineNames));
    }

    public IEnumerable<Identifier> TableNames { get; }

    public IEnumerable<Identifier> ViewNames { get; }

    public IEnumerable<Identifier> SequenceNames { get; }

    public IEnumerable<Identifier> SynonymNames { get; }

    public IEnumerable<Identifier> RoutineNames { get; }
}
