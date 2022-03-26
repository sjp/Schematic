using NUnit.Framework;

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
}