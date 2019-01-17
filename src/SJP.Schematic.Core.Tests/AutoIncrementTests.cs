using System;
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

            Assert.Throws<ArgumentException>(() => new AutoIncrement(initialValue, increment));
        }

        [Test]
        public static void InitialValue_PropertyGet_EqualsCtorArgument()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var autoIncrement = new AutoIncrement(initialValue, increment);

            Assert.AreEqual(initialValue, autoIncrement.InitialValue);
        }

        [Test]
        public static void Increment_PropertyGet_EqualsCtorArgument()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var autoIncrement = new AutoIncrement(initialValue, increment);

            Assert.AreEqual(increment, autoIncrement.Increment);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            var areEqual = a.Equals(b);

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsT_GivenNullIAutoIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            var areEqual = a.Equals(null);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void Equals_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            object b = new AutoIncrement(initialValue, increment);

            var areEqual = a.Equals(b);

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void Equals_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            object b = new AutoIncrement(54321, increment);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void Equals_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            object b = new AutoIncrement(initialValue, 6789);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void Equals_GivenNonAutoIncrementObject_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new object();

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void GetHashCode_GivenObjectsWithEqualInputs_AreEqual()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            var aHash = a.GetHashCode();
            var bHash = b.GetHashCode();

            Assert.AreEqual(aHash, bHash);
        }

        [Test]
        public static void GetHashCode_GivenObjectsWithDifferentInitialValue_AreNotEqual()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            var aHash = a.GetHashCode();
            var bHash = b.GetHashCode();

            Assert.AreNotEqual(aHash, bHash);
        }

        [Test]
        public static void GetHashCode_GivenObjectsWithDifferentIncrement_AreNotEqual()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            var aHash = a.GetHashCode();
            var bHash = b.GetHashCode();

            Assert.AreNotEqual(aHash, bHash);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            var areEqual = a == b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, increment);

            var areEqual = a == b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(54321, increment);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, 6789);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpRightIAutoIncrement_GivenNullIAutoIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            var areEqual = a == (FakeAutoIncrement)null;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            var areEqual = a == b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpLeftIAutoIncrement_GivenNullIAutoIncrement_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            var areEqual = (FakeAutoIncrement)null == a;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            var areEqual = a != b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithDifferentInitialValue_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithDifferentIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, increment);

            var areEqual = a != b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(54321, increment);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);
            var b = new FakeAutoIncrement(initialValue, 6789);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightIAutoIncrement_GivenNullIAutoIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            var areEqual = a != (FakeAutoIncrement)null;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, increment);

            var areEqual = a != b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentInitialValue_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(54321, increment);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenObjectsWithDifferentIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new FakeAutoIncrement(initialValue, increment);
            var b = new AutoIncrement(initialValue, 6789);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftIAutoIncrement_GivenNullIAutoIncrement_ReturnsTrue()
        {
            const int initialValue = 12345;
            const int increment = 9876;
            var a = new AutoIncrement(initialValue, increment);

            var areEqual = (FakeAutoIncrement)null != a;

            Assert.IsTrue(areEqual);
        }

        private class FakeAutoIncrement : IAutoIncrement
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
