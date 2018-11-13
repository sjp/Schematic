using System;
using System.Data;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseSequenceTests
    {
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var sequenceName = new Identifier("test", "test_sequence");
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(null, sequenceName));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(connection, null));
        }

        [Test]
        public static void Ctor_GivenNameMissingSchema_ThrowsArgException()
        {
            var connection = Mock.Of<IDbConnection>();
            const string sequenceName = "test_sequence";

            Assert.Throws<ArgumentException>(() => new OracleDatabaseSequence(connection, sequenceName));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var sequenceName = new Identifier("test", "test_sequence");

            var sequence = new OracleDatabaseSequence(connection, sequenceName);

            Assert.AreEqual(sequenceName, sequence.Name);
        }
    }
}
