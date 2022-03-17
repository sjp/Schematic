using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class ObjectParameterTransformerTests
{
    [Test]
    public static void ToDictionary_GivenNullObject_ThrowsArgumentNullException()
    {
        Assert.That(() => ObjectParameterTransformer.ToDictionary(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToDictionary_GivenEmptyObject_ReturnsEmptyDictionary()
    {
        var lookup = ObjectParameterTransformer.ToDictionary(new { });

        Assert.That(lookup, Is.Empty);
    }

    [Test]
    public static void ToDictionary_GivenNonEmptyObject_ReturnsDictionaryWithCorrectKeysAndValues()
    {
        const int first = 1;
        const string second = "second";
        var lookup = ObjectParameterTransformer.ToDictionary(new { FirstKey = first, SecondKey = second });

        Assert.Multiple(() =>
        {
            Assert.That(lookup, Has.Exactly(2).Items);
            Assert.That(lookup["FirstKey"], Is.EqualTo(first));
            Assert.That(lookup["SecondKey"], Is.EqualTo(second));
        });
    }

    [Test]
    public static void ToParameters_GivenNullLookup_ThrowsArgumentNullException()
    {
        Assert.That(() => ObjectParameterTransformer.ToParameters(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ToParameters_GivenEmptyLookup_ReturnsEmptyDictionary()
    {
        var parameters = ObjectParameterTransformer.ToParameters(new Dictionary<string, object>(StringComparer.Ordinal));

        Assert.That(parameters.ParameterNames, Is.Empty);
    }

    [Test]
    public static void ToParameters_GivenNonEmptyLookup_ReturnsParametersWithCorrectKeysAndValues()
    {
        const int first = 1;
        const string second = "second";

        var lookup = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["FirstKey"] = first,
            ["SecondKey"] = second
        };
        var parameters = ObjectParameterTransformer.ToParameters(lookup);
        var paramCount = parameters.ParameterNames.Count();

        Assert.Multiple(() =>
        {
            Assert.That(paramCount, Is.EqualTo(2));
            Assert.That(parameters.Get<int>("FirstKey"), Is.EqualTo(first));
            Assert.That(parameters.Get<string>("SecondKey"), Is.EqualTo(second));
        });
    }
}
