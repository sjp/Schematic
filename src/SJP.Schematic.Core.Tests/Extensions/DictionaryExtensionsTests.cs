using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class DictionaryExtensionsTests
{
    [Test]
    public static void ToDictionary_GivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<KeyValuePair<string, string>> input = null;
        Assert.That(() => input.ToDictionary(), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToDictionary_GivenEmptyCollection_ReturnsEmptyDictionary()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        var result = input.ToDictionary();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void ToDictionary_GivenNonEmptyCollection_ReturnsDictionaryWithEqualKeysAndValues()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a"] = "A",
            ["b"] = "B"
        };
        var result = input.ToDictionary();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContainsKey("a"), Is.True);
            Assert.That(result.ContainsKey("b"), Is.True);
            Assert.That(result["a"], Is.EqualTo("A"));
            Assert.That(result["b"], Is.EqualTo("B"));
        }
    }

    [Test]
    public static void ToDictionary_WithComparerGivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<KeyValuePair<string, string>> input = null;
        Assert.That(() => input.ToDictionary(StringComparer.Ordinal), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToDictionary_WithComparerGivenNullComparer_ThrowsArgumentNullException()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        Assert.That(() => input.ToDictionary(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToDictionary_WithComparerGivenEmptyCollection_ReturnsEmptyDictionary()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        var result = input.ToDictionary(StringComparer.Ordinal);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public static void ToDictionary_WithComparerGivenNonEmptyCollection_ReturnsDictionaryWithEqualKeysAndValues()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a"] = "A",
            ["b"] = "B"
        };
        var result = input.ToDictionary(StringComparer.OrdinalIgnoreCase);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContainsKey("A"), Is.True);
            Assert.That(result.ContainsKey("B"), Is.True);
            Assert.That(result["A"], Is.EqualTo("A"));
            Assert.That(result["B"], Is.EqualTo("B"));
        }
    }

    [Test]
    public static void ToReadOnlyDictionary_GivenNullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<KeyValuePair<string, string>> input = null;
        Assert.That(() => input.ToReadOnlyDictionary(), Throws.ArgumentNullException);
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
            ["b"] = "B"
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
        Assert.That(() => input.ToReadOnlyDictionary(StringComparer.Ordinal), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToReadOnlyDictionary_WithComparerGivenNullComparer_ThrowsArgumentNullException()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal);
        Assert.That(() => input.ToReadOnlyDictionary(null), Throws.ArgumentNullException);
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
            ["b"] = "B"
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
}