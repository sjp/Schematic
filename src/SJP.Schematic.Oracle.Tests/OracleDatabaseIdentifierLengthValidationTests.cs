using System;
using NUnit.Framework;
using System.Data;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseIdentifierLengthValidationTests
    {
        private static IOracleDatabaseIdentifierValidation Validator => new OracleDatabaseIdentifierLengthValidation(30);

        [Test]
        public static void Ctor_GivenZeroLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new OracleDatabaseIdentifierLengthValidation(0));
        }

        [Test]
        public static void IsValidIdentifier_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Validator.IsValidIdentifier(null));
        }

        [Test]
        public static void IsValidIdentifier_GivenIdentifierWithShortName_ReturnsTrue()
        {
            Identifier identifier = "test";

            var isValid = Validator.IsValidIdentifier(identifier);

            Assert.IsTrue(isValid);
        }

        [Test]
        public static void IsValidIdentifier_GivenIdentifierWithNonAsciiName_ReturnsFalse()
        {
            Identifier identifier = "☃";

            var isValid = Validator.IsValidIdentifier(identifier);

            Assert.IsFalse(isValid);
        }

        [Test]
        public static void IsValidIdentifier_GivenIdentifierWithLongName_ReturnsFalse()
        {
            // really long, should exceed max length even for newer oracle databases
            Identifier identifier = "test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test";

            var isValid = Validator.IsValidIdentifier(identifier);

            Assert.IsFalse(isValid);
        }
    }
}
