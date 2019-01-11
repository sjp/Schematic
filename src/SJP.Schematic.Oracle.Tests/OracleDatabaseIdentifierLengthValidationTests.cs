using System;
using NUnit.Framework;
using System.Data;
using Moq;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseIdentifierLengthValidationTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseIdentifierLengthValidation(null));
        }

        [Test]
        public static void IsValidIdentifier_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var validation = new OracleDatabaseIdentifierLengthValidation(connection);

            Assert.Throws<ArgumentNullException>(() => validation.IsValidIdentifier(null));
        }
    }
}
