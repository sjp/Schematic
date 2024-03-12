using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dot.Tests;

[TestFixture]
internal static class DotFormatterTests
{
    private static IIdentifierDefaults IdentifierDefaults { get; } = new IdentifierDefaults("server", "database", "schema");

    private static IDotFormatter Formatter { get; } = new DotFormatter(IdentifierDefaults);

    [Test]
    public static void Ctor_GivenNullDefaults_ThrowsArgumentNullException()
    {
        Assert.That(() => new DotFormatter(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void RenderTables_GivenNullTables_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullTablesWithValidOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables(null, new DotRenderOptions()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullOptionsWithValidTables_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables([], (DotRenderOptions)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullTablesWithValidRowCounts_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables(null, new Dictionary<Identifier, ulong>()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullTablesWithValidRowCountsAndOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables(null, new Dictionary<Identifier, ulong>(), new DotRenderOptions()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullRowCountsWithValidTablesAndOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables([], null, new DotRenderOptions()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullOptionsWithTablesAndValidRowCounts_ThrowsArgumentNullException()
    {
        Assert.That(() => Formatter.RenderTables([], new Dictionary<Identifier, ulong>(), null), Throws.ArgumentNullException);
    }
}