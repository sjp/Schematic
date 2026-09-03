using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class ReportDataTests
{
    [Test]
    public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(null!, [], [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), null!, [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullViews_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], null!, [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullSequences_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], null!, [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], null!, [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullRoutines_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], null!, [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullSchemas_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], null!, "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullReferencedObjectTargets_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], "1.0", null!, EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullSynonymTargets_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], "1.0", EmptyTargets(), null!, EmptyStatistics()),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDatabaseVersion_DoesNotThrow()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], null, EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNullTableStatistics_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), null!),
            Throws.ArgumentNullException);
    }

    private static IRelationalDatabase MockDatabase() => new Mock<IRelationalDatabase>().Object;

    private static ReferencedObjectTargets EmptyTargets() => new(new Mock<IDependencyProvider>().Object, [], [], [], [], []);

    private static SynonymTargets EmptySynonymTargets() => new([], [], [], [], []);

    private static IReadOnlyDictionary<Identifier, ITableStatistics> EmptyStatistics() => new Dictionary<Identifier, ITableStatistics>();
}
