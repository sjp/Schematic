using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class DefensiveCopyExtensionsTests
{
    [Test]
    public static void ToDefensiveCopy_GivenNullDictionary_ThrowsArgumentNullExceptionWithParamName()
    {
        IReadOnlyDictionary<string, string> input = null;

        Assert.That(
            () => input.ToDefensiveCopy("lookup"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("lookup")
        );
    }

    [Test]
    public static void ToDefensiveCopy_GivenFrozenDictionary_ReturnsSameInstance()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal) { ["a"] = "A" }.ToFrozenDictionary(StringComparer.Ordinal);

        var result = input.ToDefensiveCopy(nameof(input));

        Assert.That(result, Is.SameAs(input));
    }

    [Test]
    public static void ToDefensiveCopy_GivenEmptyDictionaries_ReturnsSharedEmptyInstance()
    {
        var first = new Dictionary<string, string>(StringComparer.Ordinal);
        var second = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var firstResult = first.ToDefensiveCopy(nameof(first));
        var secondResult = second.ToDefensiveCopy(nameof(second));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstResult, Is.Empty);
            Assert.That(secondResult, Is.SameAs(firstResult));
        }
    }

    [Test]
    public static void ToDefensiveCopy_GivenDictionary_PreservesComparer()
    {
        var input = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["a"] = "A" };

        var result = input.ToDefensiveCopy(nameof(input));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContainsKey("A"), Is.True);
            Assert.That(result["A"], Is.EqualTo("A"));
        }
    }

    [Test]
    public static void ToDefensiveCopy_WhenSourceModifiedAfterCopy_DoesNotReflectChanges()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal) { ["a"] = "A" };

        var result = input.ToDefensiveCopy(nameof(input));
        input["b"] = "B";
        input["a"] = "changed";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["a"], Is.EqualTo("A"));
        }
    }
}
