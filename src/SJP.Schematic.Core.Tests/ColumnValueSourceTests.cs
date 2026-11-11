using System;
using LanguageExt;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

internal static class ColumnValueSourceTests
{
    [Test]
    public static void ComputedCtor_GivenInvalidStorage_ThrowsArgumentException()
    {
        const ComputedColumnStorage storage = (ComputedColumnStorage)55;

        Assert.That(
            () => new ColumnValueSource.Computed(Option<string>.Some("1"), storage),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("storage")
        );
    }

    [Test]
    public static void GeneratedCtor_GivenNullIdentity_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ColumnValueSource.Generated(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identity")
        );
    }

    [Test]
    public static void Definition_GivenSomeDefinition_EqualsCtorArg()
    {
        const string expression = "a + b";
        var computed = new ColumnValueSource.Computed(Option<string>.Some(expression), ComputedColumnStorage.Stored);

        Assert.That(computed.Definition, Is.EqualTo(Option<string>.Some(expression)));
    }

    [Test]
    public static void Definition_GivenNoneDefinition_EqualsNone()
    {
        var computed = new ColumnValueSource.Computed(Option<string>.None, ComputedColumnStorage.Virtual);

        Assert.That(computed.Definition.IsNone, Is.True);
    }

    [TestCase(ComputedColumnStorage.Unknown)]
    [TestCase(ComputedColumnStorage.Virtual)]
    [TestCase(ComputedColumnStorage.Stored)]
    public static void Storage_PropertyGet_EqualsCtorArg(ComputedColumnStorage storage)
    {
        var computed = new ColumnValueSource.Computed(Option<string>.Some("1"), storage);

        Assert.That(computed.Storage, Is.EqualTo(storage));
    }

    [Test]
    public static void Identity_PropertyGet_EqualsCtorArg()
    {
        var identity = new AutoIncrement(10, 5);
        var generated = new ColumnValueSource.Generated(identity);

        Assert.That(generated.Identity, Is.SameAs(identity));
    }

    [Test]
    public static void Equals_GivenStoredCases_ReturnsTrue()
    {
        ColumnValueSource first = new ColumnValueSource.Stored();
        ColumnValueSource second = new ColumnValueSource.Stored();

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenComputedCasesWithSameMembers_ReturnsTrue()
    {
        ColumnValueSource first = new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Stored);
        ColumnValueSource second = new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Stored);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenComputedCasesWithDifferentStorage_ReturnsFalse()
    {
        ColumnValueSource first = new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Stored);
        ColumnValueSource second = new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Virtual);

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenGeneratedCasesWithEqualIdentities_ReturnsTrue()
    {
        ColumnValueSource first = new ColumnValueSource.Generated(new AutoIncrement(1, 1));
        ColumnValueSource second = new ColumnValueSource.Generated(new AutoIncrement(1, 1));

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public static void Equals_GivenDifferentCases_ReturnsFalse()
    {
        ColumnValueSource first = new ColumnValueSource.Stored();
        ColumnValueSource second = new ColumnValueSource.Generated(new AutoIncrement(1, 1));

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public static void ToString_WhenComputed_ContainsDefinition()
    {
        ColumnValueSource valueSource = new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Stored);

        Assert.That(valueSource.ToString(), Does.Contain("a + b"));
    }

    [Test]
    public static void ToString_WhenDefault_ReturnsEmptyString()
    {
        ColumnValueSource valueSource = default;

        Assert.That(valueSource.ToString(), Is.Empty);
    }
}
