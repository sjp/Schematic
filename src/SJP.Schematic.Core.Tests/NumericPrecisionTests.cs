using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class NumericPrecisionTests
    {
        [Test]
        public static void Ctor_GivenNegativePrecision_ThrowsArgumentException()
        {
            const int precision = -1;
            const int scale = 123;

            Assert.Throws<ArgumentException>(() => new NumericPrecision(precision, scale));
        }

        [Test]
        public static void Ctor_GivenNegativeScale_ThrowsArgumentException()
        {
            const int precision = 12345;
            const int scale = -1;

            Assert.Throws<ArgumentException>(() => new NumericPrecision(precision, scale));
        }

        [Test]
        public static void Precision_PropertyGet_EqualsCtorArgument()
        {
            const int precision = 12345;
            const int scale = 9876;
            var autoScale = new NumericPrecision(precision, scale);

            Assert.AreEqual(precision, autoScale.Precision);
        }

        [Test]
        public static void Scale_PropertyGet_EqualsCtorArgument()
        {
            const int precision = 12345;
            const int scale = 9876;
            var autoScale = new NumericPrecision(precision, scale);

            Assert.AreEqual(scale, autoScale.Scale);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, scale);

            var areEqual = a.Equals(b);

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithDifferentPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(54321, scale);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsT_GivenObjectsWithDifferentScale_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, 6789);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsT_GivenNullINumericPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);

            var areEqual = a.Equals(null);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void Equals_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            object b = new NumericPrecision(precision, scale);

            var areEqual = a.Equals(b);

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void Equals_GivenObjectsWithDifferentPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            object b = new NumericPrecision(54321, scale);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void Equals_GivenObjectsWithDifferentScale_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            object b = new NumericPrecision(precision, 6789);

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void Equals_GivenNonNumericPrecisionObject_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new object();

            var areEqual = a.Equals(b);

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void GetHashCode_GivenObjectsWithEqualInputs_AreEqual()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, scale);

            var aHash = a.GetHashCode();
            var bHash = b.GetHashCode();

            Assert.AreEqual(aHash, bHash);
        }

        [Test]
        public static void GetHashCode_GivenObjectsWithDifferentPrecision_AreNotEqual()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(54321, scale);

            var aHash = a.GetHashCode();
            var bHash = b.GetHashCode();

            Assert.AreNotEqual(aHash, bHash);
        }

        [Test]
        public static void GetHashCode_GivenObjectsWithDifferentScale_AreNotEqual()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, 6789);

            var aHash = a.GetHashCode();
            var bHash = b.GetHashCode();

            Assert.AreNotEqual(aHash, bHash);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, scale);

            var areEqual = a == b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithDifferentPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(54321, scale);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOp_GivenObjectsWithDifferentScale_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, 6789);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpRightINumericPrecision_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new FakeNumericPrecision(precision, scale);

            var areEqual = a == b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsOpRightINumericPrecision_GivenObjectsWithDifferentPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new FakeNumericPrecision(54321, scale);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpRightINumericPrecision_GivenObjectsWithDifferentScale_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new FakeNumericPrecision(precision, 6789);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpRightINumericPrecision_GivenNullINumericPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);

            var areEqual = a == (FakeNumericPrecision)null;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpLeftINumericPrecision_GivenObjectsWithEqualInputs_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new FakeNumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, scale);

            var areEqual = a == b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void EqualsOpLeftINumericPrecision_GivenObjectsWithDifferentPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new FakeNumericPrecision(precision, scale);
            var b = new NumericPrecision(54321, scale);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpLeftINumericPrecision_GivenObjectsWithDifferentScale_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new FakeNumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, 6789);

            var areEqual = a == b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void EqualsOpLeftINumericPrecision_GivenNullINumericPrecision_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);

            var areEqual = (FakeNumericPrecision)null == a;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, scale);

            var areEqual = a != b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithDifferentPrecision_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(54321, scale);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOp_GivenObjectsWithDifferentScale_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, 6789);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightINumericPrecision_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new FakeNumericPrecision(precision, scale);

            var areEqual = a != b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightINumericPrecision_GivenObjectsWithDifferentPrecision_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new FakeNumericPrecision(54321, scale);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightINumericPrecision_GivenObjectsWithDifferentScale_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);
            var b = new FakeNumericPrecision(precision, 6789);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpRightINumericPrecision_GivenNullINumericPrecision_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);

            var areEqual = a != (FakeNumericPrecision)null;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftINumericPrecision_GivenObjectsWithEqualInputs_ReturnsFalse()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new FakeNumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, scale);

            var areEqual = a != b;

            Assert.IsFalse(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftINumericPrecision_GivenObjectsWithDifferentPrecision_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new FakeNumericPrecision(precision, scale);
            var b = new NumericPrecision(54321, scale);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftINumericPrecision_GivenObjectsWithDifferentScale_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new FakeNumericPrecision(precision, scale);
            var b = new NumericPrecision(precision, 6789);

            var areEqual = a != b;

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void NotEqualsOpLeftINumericPrecision_GivenNullINumericPrecision_ReturnsTrue()
        {
            const int precision = 12345;
            const int scale = 9876;
            var a = new NumericPrecision(precision, scale);

            var areEqual = (FakeNumericPrecision)null != a;

            Assert.IsTrue(areEqual);
        }

        private sealed class FakeNumericPrecision : INumericPrecision
        {
            public FakeNumericPrecision(int precision, int scale)
            {
                Precision = precision;
                Scale = scale;
            }

            public int Precision { get; }

            public int Scale { get; }
        }
    }
}
