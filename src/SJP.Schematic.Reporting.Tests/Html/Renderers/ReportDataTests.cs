using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

internal static class ReportDataTests
{
    [Test]
    public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(null!, [], [], [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("database"));
    }

    [Test]
    public static void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), null!, [], [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tables"));
    }

    [Test]
    public static void Ctor_GivenNullViews_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], null!, [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("views"));
    }

    [Test]
    public static void Ctor_GivenNullSequences_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], null!, [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequences"));
    }

    [Test]
    public static void Ctor_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], null!, [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonyms"));
    }

    [Test]
    public static void Ctor_GivenNullRoutines_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], null!, [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routines"));
    }

    [Test]
    public static void Ctor_GivenNullSchemas_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], null!, [], "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("schemas"));
    }

    [Test]
    public static void Ctor_GivenNullUserDefinedTypes_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], null!, "1.0", EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("userDefinedTypes"));
    }

    [Test]
    public static void Ctor_GivenNullReferencedObjectTargets_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], [], "1.0", null!, EmptySynonymTargets(), EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("referencedObjectTargets"));
    }

    [Test]
    public static void Ctor_GivenNullSynonymTargets_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], [], "1.0", EmptyTargets(), null!, EmptyStatistics()),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymTargets"));
    }

    [Test]
    public static void Ctor_GivenNullDatabaseVersion_DoesNotThrow()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], [], null, EmptyTargets(), EmptySynonymTargets(), EmptyStatistics()),
            Throws.Nothing);
    }

    [Test]
    public static void Ctor_GivenNullTableStatistics_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportData(MockDatabase(), [], [], [], [], [], [], [], "1.0", EmptyTargets(), EmptySynonymTargets(), null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableStatistics"));
    }

    private static IRelationalDatabase MockDatabase() => new Mock<IRelationalDatabase>().Object;

    private static ReferencedObjectTargets EmptyTargets() => new(new EmptyDependencyProvider(), [], [], [], [], [], []);

    private static SynonymTargets EmptySynonymTargets() => new([], [], [], [], [], []);

    private static IReadOnlyDictionary<Identifier, ITableStatistics> EmptyStatistics() => new Dictionary<Identifier, ITableStatistics>();
}
