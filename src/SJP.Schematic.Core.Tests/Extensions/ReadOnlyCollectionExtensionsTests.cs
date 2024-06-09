using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class ReadOnlyCollectionExtensionsTests
{
    [Test]
    public static void Empty_GivenNullCollection_ThrowsArgumentNullException()
    {
        IReadOnlyCollection<string> input = null;
        Assert.That(() => input.Empty(), Throws.ArgumentNullException);
    }

    [Test]
    public static void Empty_GivenEmptyCollection_ReturnsTrue()
    {
        IReadOnlyCollection<string> input = [];
        Assert.That(input.Empty(), Is.True);
    }

    [Test]
    public static void Empty_GivenNonEmptyCollection_ReturnsFalse()
    {
        IReadOnlyCollection<string> input = ["A"];
        Assert.That(input.Empty(), Is.False);
    }
}