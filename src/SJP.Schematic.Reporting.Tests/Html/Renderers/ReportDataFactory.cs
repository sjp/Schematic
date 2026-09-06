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
        IReadOnlyCollection<IDatabaseSchema> schemas = null!,
        IReadOnlyCollection<IDatabaseUserDefinedType> userDefinedTypes = null!,
        string databaseVersion = null!,
        ReferencedObjectTargets referencedObjectTargets = null!,
        SynonymTargets synonymTargets = null!,
        IReadOnlyDictionary<Identifier, ITableStatistics> tableStatistics = null!)
    {
        return new ReportData(
            database ?? DefaultDatabase(),
            tables ?? [],
            views ?? [],
            sequences ?? [],
            synonyms ?? [],
            routines ?? [],
            schemas ?? [],
            userDefinedTypes ?? [],
            databaseVersion,
            referencedObjectTargets ?? new ReferencedObjectTargets(new Mock<IDependencyProvider>().Object, [], [], [], [], []),
            synonymTargets ?? new SynonymTargets([], [], [], [], []),
            tableStatistics ?? new Dictionary<Identifier, ITableStatistics>());
    }

    // A database always has identifier defaults, and renderers rely on them (e.g. to decide which
    // schema is the default one), so the stand-in supplies empty ones rather than null.
    private static IRelationalDatabase DefaultDatabase()
    {
        var database = new Mock<IRelationalDatabase>();
        database.Setup(static db => db.IdentifierDefaults).Returns(IdentifierDefaults.Empty);
        return database.Object;
    }
}
