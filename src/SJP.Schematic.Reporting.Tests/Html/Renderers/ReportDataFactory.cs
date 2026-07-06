using System.Collections.Generic;
using Moq;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

// Renderers take their input as a ReportData parameter (see IDataRenderer) rather than via their
// constructor, so a single ReportData can be assembled here with sensible empty defaults and each
// test overrides only the fields it cares about.
internal static class ReportDataFactory
{
    public static ReportData Create(
        IRelationalDatabase database = null!,
        IReadOnlyCollection<IRelationalDatabaseTable> tables = null!,
        IReadOnlyCollection<IDatabaseView> views = null!,
        IReadOnlyCollection<IDatabaseSequence> sequences = null!,
        IReadOnlyCollection<IDatabaseSynonym> synonyms = null!,
        IReadOnlyCollection<IDatabaseRoutine> routines = null!,
        IReadOnlyDictionary<Identifier, ulong> rowCounts = null!,
        string databaseVersion = null!,
        ReferencedObjectTargets referencedObjectTargets = null!,
        SynonymTargets synonymTargets = null!)
    {
        return new ReportData(
            database ?? new Mock<IRelationalDatabase>().Object,
            tables ?? [],
            views ?? [],
            sequences ?? [],
            synonyms ?? [],
            routines ?? [],
            rowCounts ?? new Dictionary<Identifier, ulong>(),
            databaseVersion,
            referencedObjectTargets ?? new ReferencedObjectTargets(new Mock<IDependencyProvider>().Object, [], [], [], [], []),
            synonymTargets ?? new SynonymTargets([], [], [], [], []));
    }
}
