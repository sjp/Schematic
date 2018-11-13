using System;
using NUnit.Framework;
using Moq;
using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseSequenceTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            var sequenceName = new Identifier("test", "test_sequence");

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSequence(null, sequenceName));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSequence(connection, null));
        }

        [Test]
        public static void Ctor_GivenNameMissingSchema_ThrowsArgumentException()
        {
            var connection = Mock.Of<IDbConnection>();
            const string sequenceName = "test_sequence";

            Assert.Throws<ArgumentException>(() => new SqlServerDatabaseSequence(connection, sequenceName));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var sequenceName = new Identifier("test", "test_sequence");

            var sequence = new SqlServerDatabaseSequence(connection, sequenceName);

            Assert.AreEqual(sequenceName, sequence.Name);
        }
    }
}
