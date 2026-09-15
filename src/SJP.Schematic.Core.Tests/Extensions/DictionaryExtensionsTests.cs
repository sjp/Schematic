using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

internal static class DictionaryExtensionsTests
{
    [Test]
    public static void ToReadOnlyDictionary_GivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<KeyValuePair<string, string>> input = null;
        Assert.That(
            () => input.ToReadOnlyDictionary(),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void ToReadOnlyDictionary_GivenEmptyCollection_ReturnsEmptyDictionary()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        var result = input.ToReadOnlyDictionary();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void ToReadOnlyDictionary_GivenNonEmptyCollection_ReturnsDictionaryWithEqualKeysAndValues()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a"] = "A",
            ["b"] = "B",
        };
        var result = input.ToReadOnlyDictionary();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContainsKey("a"), Is.True);
            Assert.That(result.ContainsKey("b"), Is.True);
            Assert.That(result["a"], Is.EqualTo("A"));
            Assert.That(result["b"], Is.EqualTo("B"));
        }
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<KeyValuePair<string, string>> input = null;
        Assert.That(
            () => input.ToReadOnlyDictionary(StringComparer.Ordinal),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenNullComparer_ThrowsArgumentNullException()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        Assert.That(
            () => input.ToReadOnlyDictionary(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("comparer")
        );
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenEmptyCollection_ReturnsEmptyDictionary()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        var result = input.ToReadOnlyDictionary(StringComparer.Ordinal);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenNonEmptyCollection_ReturnsDictionaryWithEqualKeysAndValues()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a"] = "A",
            ["b"] = "B",
        };
        var result = input.ToReadOnlyDictionary(StringComparer.OrdinalIgnoreCase);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContainsKey("A"), Is.True);
            Assert.That(result.ContainsKey("B"), Is.True);
            Assert.That(result["A"], Is.EqualTo("A"));
            Assert.That(result["B"], Is.EqualTo("B"));
        }
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenKeysEqualUnderComparer_ThrowsArgumentException()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a"] = "A",
            ["A"] = "B",
        };

        Assert.That(() => input.ToReadOnlyDictionary(StringComparer.OrdinalIgnoreCase), Throws.ArgumentException);
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenNonDictionarySource_ReturnsDictionaryWithEqualKeysAndValues()
    {
        var input = new[]
        {
            new KeyValuePair<string, string>("a", "A"),
            new KeyValuePair<string, string>("b", "B"),
        };
        var result = input.ToReadOnlyDictionary(StringComparer.OrdinalIgnoreCase);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result["A"], Is.EqualTo("A"));
            Assert.That(result["B"], Is.EqualTo("B"));
        }
    }

    [Test]
    public static void ToReadOnlyDictionary_WhenSourceModifiedAfterCopy_DoesNotReflectChanges()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal) { ["a"] = "A" };

        var result = input.ToReadOnlyDictionary(StringComparer.Ordinal);
        input["a"] = "changed";

        Assert.That(result["a"], Is.EqualTo("A"));
    }
}