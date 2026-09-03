using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class AutoIncrementTests
{
    [Test]
    public static void Ctor_GivenZeroIncrement_ThrowsArgumentException()
    {
        const int initialValue = 12345;
        const int increment = 0;

        Assert.That(() => new AutoIncrement(initialValue, increment), Throws.ArgumentException);
    }

    [Test]
    public static void InitialValue_PropertyGet_EqualsCtorArgument()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var autoIncrement = new AutoIncrement(initialValue, increment);

        Assert.That(autoIncrement.InitialValue, Is.EqualTo(initialValue));
    }

    [Test]
    public static void Increment_PropertyGet_EqualsCtorArgument()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var autoIncrement = new AutoIncrement(initialValue, increment);

        Assert.That(autoIncrement.Increment, Is.EqualTo(increment));
    }

    [Test]
    public static void EqualsT_GivenObjectsWithEqualInputs_ReturnsTrue()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new AutoIncrement(initialValue, increment);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public static void EqualsT_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new AutoIncrement(54321, increment);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void EqualsT_GivenObjectsWithDifferentIncrement_ReturnsFalse()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new AutoIncrement(initialValue, 6789);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void EqualsT_GivenNullIAutoIncrement_ReturnsFalse()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);

        // note: don't update this to Is.Not.Null to ensure the method is run correctly
        Assert.That(a, Is.Not.EqualTo(null));
    }

    [Test]
    public static void Equals_GivenObjectsWithEqualInputs_ReturnsTrue()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        object b = new AutoIncrement(initialValue, increment);

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public static void Equals_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        object b = new AutoIncrement(54321, increment);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void Equals_GivenObjectsWithDifferentIncrement_ReturnsFalse()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        object b = new AutoIncrement(initialValue, 6789);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void Equals_GivenNonAutoIncrementObject_ReturnsFalse()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new object();

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void GetHashCode_GivenObjectsWithEqualInputs_AreEqual()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new AutoIncrement(initialValue, increment);

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void GetHashCode_GivenObjectsWithDifferentInitialValue_AreNotEqual()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new AutoIncrement(54321, increment);

        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void GetHashCode_GivenObjectsWithDifferentIncrement_AreNotEqual()
    {
        const int initialValue = 12345;
        const int increment = 9876;
        var a = new AutoIncrement(initialValue, increment);
        var b = new AutoIncrement(initialValue, 6789);

        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void Ctor_GivenInvalidGeneration_ThrowsArgumentException()
    {
        const IdentityGeneration generation = (IdentityGeneration)55;

        Assert.That(
            () => new AutoIncrement(1, 1, generation, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None),
            Throws.ArgumentException
        );
    }

    [Test]
    public static void Generation_GivenTwoArgumentCtor_IsByDefault()
    {
        var autoIncrement = new AutoIncrement(1, 1);

        Assert.That(autoIncrement.Generation, Is.EqualTo(IdentityGeneration.ByDefault));
    }

    [Test]
    public static void Bounds_GivenTwoArgumentCtor_AreNone()
    {
        var autoIncrement = new AutoIncrement(1, 1);

        Assert.Multiple(() =>
        {
            Assert.That(autoIncrement.MinValue, OptionIs.None);
            Assert.That(autoIncrement.MaxValue, OptionIs.None);
            Assert.That(autoIncrement.SequenceName, OptionIs.None);
            Assert.That(autoIncrement.Cycle, Is.False);
        });
    }

    [Test]
    public static void IdentityProperties_PropertyGet_EqualCtorArguments()
    {
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_sequence");
        var autoIncrement = new AutoIncrement(
            12345,
            9876,
            IdentityGeneration.Always,
            Option<decimal>.Some(-100),
            Option<decimal>.Some(100),
            true,
            Option<Identifier>.Some(sequenceName)
        );

        Assert.Multiple(() =>
        {
            Assert.That(autoIncrement.Generation, Is.EqualTo(IdentityGeneration.Always));
            Assert.That(autoIncrement.MinValue.UnwrapSome(), Is.EqualTo(-100));
            Assert.That(autoIncrement.MaxValue.UnwrapSome(), Is.EqualTo(100));
            Assert.That(autoIncrement.Cycle, Is.True);
            Assert.That(autoIncrement.SequenceName.UnwrapSome(), Is.EqualTo(sequenceName));
        });
    }

    [Test]
    public static void EqualsT_GivenObjectsWithDifferentGeneration_ReturnsFalse()
    {
        var a = new AutoIncrement(1, 1, IdentityGeneration.Always, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None);
        var b = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void EqualsT_GivenObjectsWithDifferentSequenceName_ReturnsFalse()
    {
        var a = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.Some("seq_a"));
        var b = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.Some("seq_b"));

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public static void EqualsT_GivenObjectsWithDifferentBoundsOrCycle_ReturnsFalse()
    {
        var a = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.Some(1), Option<decimal>.Some(100), false, Option<Identifier>.None);
        var differentMin = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.Some(2), Option<decimal>.Some(100), false, Option<Identifier>.None);
        var differentMax = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.Some(1), Option<decimal>.Some(200), false, Option<Identifier>.None);
        var differentCycle = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.Some(1), Option<decimal>.Some(100), true, Option<Identifier>.None);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.Not.EqualTo(differentMin));
            Assert.That(a, Is.Not.EqualTo(differentMax));
            Assert.That(a, Is.Not.EqualTo(differentCycle));
        });
    }

    [Test]
    public static void GetHashCode_GivenObjectsWithDifferentGeneration_AreNotEqual()
    {
        var a = new AutoIncrement(1, 1, IdentityGeneration.Always, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None);
        var b = new AutoIncrement(1, 1, IdentityGeneration.ByDefault, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None);

        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }
}
