using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

/// <summary>
/// The full set of database objects for a single report run, plus the lookups derived from them
/// (<see cref="ReferencedObjectTargets"/>, <see cref="SynonymTargets"/>). Renderers receive this as
/// a <see cref="IDataRenderer.RenderAsync"/> parameter rather than via their constructor, so a
/// single renderer instance can be reused across calls and tested without rebuilding it per case.
/// </summary>
internal sealed class ReportData
{
    public ReportData(
        IRelationalDatabase database,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines,
        IReadOnlyDictionary<Identifier, ulong> rowCounts,
        string? databaseVersion,
        ReferencedObjectTargets referencedObjectTargets,
        SynonymTargets synonymTargets
    )
    {
        Database = database ?? throw new ArgumentNullException(nameof(database));
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
        DatabaseVersion = databaseVersion;
        ReferencedObjectTargets = referencedObjectTargets ?? throw new ArgumentNullException(nameof(referencedObjectTargets));
        SynonymTargets = synonymTargets ?? throw new ArgumentNullException(nameof(synonymTargets));
    }

    public IRelationalDatabase Database { get; }

    public IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    public IReadOnlyCollection<IDatabaseView> Views { get; }

    public IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

    public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

    public IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

    public IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

    public string? DatabaseVersion { get; }

    public ReferencedObjectTargets ReferencedObjectTargets { get; }

    public SynonymTargets SynonymTargets { get; }
}
