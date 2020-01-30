using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
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

            Assert.That(a.Equals(b), Is.True);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public static void EqualsT_GivenNullIAutoIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            Assert.That(a.Equals(null), Is.False);
        }

        [Test]
        public static void Equals_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            object b = new AutoIncrement(initialValue, increment);

            Assert.That(a.Equals(b), Is.True);
        }

        [Test]
        public static void Equals_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            object b = new AutoIncrement(54321, increment);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public static void Equals_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            object b = new AutoIncrement(initialValue, 6789);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public static void Equals_GivenNonAutoIncrementObject_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new object();

            Assert.That(a.Equals(b), Is.False);
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
        public static void EqualsOp_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            Assert.That(a == b, Is.True);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            Assert.That(a == b, Is.False);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            Assert.That(a == b, Is.False);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, increment);

            Assert.That(a == b, Is.True);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(54321, increment);

            Assert.That(a == b, Is.False);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, 6789);

            Assert.That(a == b, Is.False);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenNullIAutoIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            Assert.That(a == (FakeAutoIncrement)null, Is.False);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            Assert.That(a == b, Is.True);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            Assert.That(a == b, Is.False);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            Assert.That(a == b, Is.False);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenNullIAutoIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            Assert.That((FakeAutoIncrement)null == a, Is.False);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            Assert.That(a != b, Is.False);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithDifferentInitialValue_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithDifferentIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, increment);

            Assert.That(a != b, Is.False);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(54321, increment);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, 6789);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenNullIAutoIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            Assert.That(a != (FakeAutoIncrement)null, Is.True);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            Assert.That(a != b, Is.False);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenNullIAutoIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            Assert.That((FakeAutoIncrement)null != a, Is.True);
        }

        private sealed class FakeAutoIncrement : IAutoIncrement
        {
            public FakeAutoIncrement(decimal initialValue, decimal increment)
            {
                InitialValue = initialValue;
                Increment = increment;
            }

            public decimal InitialValue { get; }

            public decimal Increment { get; }
        }
    }
}
