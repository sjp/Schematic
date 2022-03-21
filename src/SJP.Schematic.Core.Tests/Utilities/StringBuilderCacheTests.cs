using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class StringBuilderCacheTests
{
    [Test]
    public static void GetStringAndRelease_GivenNullBuilder_ThrowsArgumentNullException()
    {
        Assert.That(() => StringBuilderCache.GetStringAndRelease(null), Throws.ArgumentNullException);
    }
}