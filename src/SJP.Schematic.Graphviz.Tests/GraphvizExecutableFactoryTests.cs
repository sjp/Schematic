using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests;

[TestFixture]
internal static class GraphvizExecutableFactoryTests
{
    [Test]
    public static void Ctor_GivenNullConfiguration_ThrowsArgNullException()
    {
        Assert.That(() => new GraphvizExecutableFactory(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetExecutable_WhenInvokedWithConfiguredPath_ReturnsExecutableWithGivenPath()
    {
        const string exePath = "dot_test.exe";

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>(StringComparer.Ordinal) { ["Graphviz:Dot"] = exePath })
            .Build();
        var factory = new GraphvizExecutableFactory(config);

        using var graphviz = factory.GetExecutable();

        Assert.That(graphviz.DotPath, Is.EqualTo(exePath));
    }

    [Test]
    public static void GetExecutable_WhenInvokedOnWindowsWithoutConfiguredPath_ReturnsExecutableWithTempPath()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            Assert.Pass();
            return;
        }

        var tempPath = Path.GetTempPath();
        var factory = new GraphvizExecutableFactory();

        using var graphviz = factory.GetExecutable();

        Assert.Multiple(() =>
        {
            Assert.That(graphviz.DotPath, Does.StartWith(tempPath));
            Assert.That(graphviz.DotPath, Does.EndWith("dot.exe"));
        });
    }
}
