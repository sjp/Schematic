using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerDatabaseSynonymTests
    {
        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            const string synonymName = "test_synonym";
            const string targetName = "test_target";

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonym(null, synonymName, targetName));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string targetName = "test_target";

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonym(database, null, targetName));
        }

        [Test]
        public static void Ctor_GivenNullTarget_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string synonymName = "test_synonym";

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonym(database, synonymName, null));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string synonymName = "test_synonym";
            const string targetName = "test_target";

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(synonymName, synonym.Name.LocalName);
        }

        [Test]
        public static void Target_PropertyGet_EqualsCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string synonymName = "test_synonym";
            const string targetName = "test_target";

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(targetName, synonym.Target.LocalName);
        }
    }
}
