using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseIdentifierLengthValidationTests : OracleTest
    {
        private IOracleDatabaseIdentifierValidation Validator => new OracleDatabaseIdentifierLengthValidation(Connection);

        [Test]
        public void IsValidIdentifier_GivenIdentifierWithShortName_ReturnsTrue()
        {
            Identifier identifier = "test";

            var isValid = Validator.IsValidIdentifier(identifier);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidIdentifier_GivenIdentifierWithNonAsciiName_ReturnsFalse()
        {
            Identifier identifier = "☃";

            var isValid = Validator.IsValidIdentifier(identifier);

            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidIdentifier_GivenIdentifierWithLongName_ReturnsFalse()
        {
            // really long, should exceed max length even for newer oracle databases
            Identifier identifier = "test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test_test";

            var isValid = Validator.IsValidIdentifier(identifier);

            Assert.IsFalse(isValid);
        }
    }
}
