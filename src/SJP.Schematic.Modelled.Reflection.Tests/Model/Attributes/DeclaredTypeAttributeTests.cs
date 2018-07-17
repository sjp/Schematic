using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model.Attributes.Tests
{
    [TestFixture]
    internal static class DeclaredTypeAttributeTests
    {
        [Test]
        public static void Ctor_GivenInvalidDataType_ThrowsArgumentException()
        {
            const DataType dataType = (DataType)55;
            Assert.Throws<ArgumentException>(() => new FakeDeclaredTypeAttribute(dataType));
        }

        [Test]
        public static void DataType_PropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Interval;
            var attr = new FakeDeclaredTypeAttribute(dataType);

            Assert.AreEqual(dataType, attr.DataType);
        }

        [Test]
        public static void Ctor_GivenInvalidDataTypeAndValidLength_ThrowsArgumentException()
        {
            const DataType dataType = (DataType)55;
            Assert.Throws<ArgumentException>(() => new FakeDeclaredTypeAttribute(dataType, 10, false));
        }

        [Test]
        public static void Ctor_GivenValidDataTypeAndInvalidLength_ThrowsArgumentOutOfRangeException()
        {
            const DataType dataType = DataType.Integer;
            Assert.Throws<ArgumentOutOfRangeException>(() => new FakeDeclaredTypeAttribute(dataType, -10, false));
        }

        [Test]
        public static void DataType_GivenValidDataTypeAndLengthPropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Interval;
            const int length = 10;
            var attr = new FakeDeclaredTypeAttribute(dataType, length, true);

            Assert.AreEqual(dataType, attr.DataType);
        }

        [Test]
        public static void Length_GivenValidDataTypeAndLengthPropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Interval;
            const int length = 10;
            var attr = new FakeDeclaredTypeAttribute(dataType, length, true);

            Assert.AreEqual(length, attr.Length);
        }

        [Test]
        public static void IsFixedLength_GivenValidDataTypeAndLengthPropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Interval;
            const int length = 10;
            const bool isFixedLength = true;
            var attr = new FakeDeclaredTypeAttribute(dataType, length, isFixedLength);

            Assert.AreEqual(isFixedLength, attr.IsFixedLength);
        }

        [Test]
        public static void Ctor_GivenInvalidDataTypeAndValidPrecisionAndValidScale_ThrowsArgumentException()
        {
            const DataType dataType = (DataType)55;
            const int precision = 10;
            const int scale = 5;

            Assert.Throws<ArgumentException>(() => new FakeDeclaredTypeAttribute(dataType, precision, scale));
        }

        [Test]
        public static void Ctor_GivenValidDataTypeAndInvalidPrecisionAndValidScale_ThrowsArgumentOutOfRangeException()
        {
            const DataType dataType = DataType.Integer;
            const int precision = -10;
            const int scale = 5;

            Assert.Throws<ArgumentOutOfRangeException>(() => new FakeDeclaredTypeAttribute(dataType, precision, scale));
        }

        [Test]
        public static void DataType_GivenValidDataTypeAndValidPrecisionAndInvalidScale_MatchesCtorArg()
        {
            const DataType dataType = DataType.Integer;
            const int precision = 10;
            const int scale = -5;

            Assert.Throws<ArgumentOutOfRangeException>(() => new FakeDeclaredTypeAttribute(dataType, precision, scale));
        }

        [Test]
        public static void DataType_GivenValidDataTypeAndPrecisionAndScalePropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Integer;
            const int precision = 10;
            const int scale = 5;
            var attr = new FakeDeclaredTypeAttribute(dataType, precision, scale);

            Assert.AreEqual(dataType, attr.DataType);
        }

        [Test]
        public static void Length_GivenValidDataTypeAndPrecisionAndScalePropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Integer;
            const int precision = 10;
            const int scale = 5;
            var attr = new FakeDeclaredTypeAttribute(dataType, precision, scale);

            Assert.AreEqual(precision, attr.Length);
        }

        [Test]
        public static void Precision_GivenValidDataTypeAndPrecisionAndScalePropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Integer;
            const int precision = 10;
            const int scale = 5;
            var attr = new FakeDeclaredTypeAttribute(dataType, precision, scale);

            Assert.AreEqual(precision, attr.Precision);
        }

        [Test]
        public static void Scale_GivenValidDataTypeAndLengthPropertyGet_MatchesCtorArg()
        {
            const DataType dataType = DataType.Integer;
            const int precision = 10;
            const int scale = 5;
            var attr = new FakeDeclaredTypeAttribute(dataType, precision, scale);

            Assert.AreEqual(scale, attr.Scale);
        }

        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        private sealed class FakeDeclaredTypeAttribute : DeclaredTypeAttribute
        {
            public FakeDeclaredTypeAttribute(DataType dataType) : base(dataType)
            {
            }

            public FakeDeclaredTypeAttribute(DataType dataType, int length, bool isFixedLength = false) : base(dataType, length, isFixedLength)
            {
            }

            public FakeDeclaredTypeAttribute(DataType dataType, int precision, int scale = 0) : base(dataType, precision, scale)
            {
            }
        }
    }
}
