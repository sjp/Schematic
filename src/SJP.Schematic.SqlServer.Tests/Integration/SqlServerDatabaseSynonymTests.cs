using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    [TestFixture]
    internal class SqlServerDatabaseSynonymTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonym(null, "test", "test"));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonym(Database, null, "test"));
        }

        [Test]
        public void Ctor_GivenNullTarget_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseSynonym(Database, "test", null));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Database;
            var synonym = new SqlServerDatabaseSynonym(database, "test", "test");

            Assert.AreSame(database, synonym.Database);
        }

        [Test]
        public void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var database = Database;
            var synonymName = new Identifier(database.DefaultSchema, "synonym_test_synonym_1");
            var synonym = new SqlServerDatabaseSynonym(database, synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Name);
        }

        [Test]
        public void Target_PropertyGetGivenNotNullCtorArg_ShouldEqualCtorArg()
        {
            var database = Database;
            var synonymName = new Identifier(database.DefaultSchema, "synonym_test_synonym_1");
            var synonym = new SqlServerDatabaseSynonym(database, synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Target);
        }
    }
}
