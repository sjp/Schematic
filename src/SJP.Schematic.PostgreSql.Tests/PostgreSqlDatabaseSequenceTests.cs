using System;
using System.Data;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseSequenceTests
    {
        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            var sequenceName = new Identifier("test", "test_sequence");
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseSequence(null, sequenceName));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseSequence(connection, null));
        }

        [Test]
        public static void Ctor_GivenNameMissingSchema_ThrowsArgException()
        {
            var connection = Mock.Of<IDbConnection>();
            const string sequenceName = "test_sequence";

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseSequence(connection, sequenceName));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var connection = Mock.Of<IDbConnection>();
            var sequenceName = new Identifier("test", "test_sequence");

            var sequence = new PostgreSqlDatabaseSequence(connection, sequenceName);

            Assert.AreEqual(sequenceName, sequence.Name);
        }
    }
}
