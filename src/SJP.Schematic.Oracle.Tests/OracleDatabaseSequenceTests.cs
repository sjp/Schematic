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
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseSequence(connection, null));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var sequenceName = new Identifier("test");

            var sequence = new OracleDatabaseSequence(connection, sequenceName);

            Assert.AreEqual(sequenceName, sequence.Name);
        }
    }
}
