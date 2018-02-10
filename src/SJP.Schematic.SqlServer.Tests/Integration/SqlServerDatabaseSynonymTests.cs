using System;
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
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new SqlServerDatabaseSynonym(Database, synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Name.LocalName);
        }

        [Test]
        public void Target_PropertyGetGivenNotNullCtorArg_ShouldEqualCtorArg()
        {
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new SqlServerDatabaseSynonym(Database, synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Target.LocalName);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var synonymName = new Identifier("synonym_test_synonym_1");
            var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var synonymName = new Identifier("asd", "synonym_test_synonym_1");
            var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, "asd", "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var synonymName = new Identifier("qwe", "asd", "synonym_test_synonym_1");
            var expectedSynonymName = new Identifier(database.ServerName, "qwe", "asd", "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var synonymName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");
            var expectedSynonymName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void Target_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("synonym_test_synonym_1");
            var expectedTargetName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "synonym_test_synonym_1");

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }

        [Test]
        public void Target_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("asd", "synonym_test_synonym_1");
            var expectedTargetName = new Identifier(database.ServerName, database.DatabaseName, "asd", "synonym_test_synonym_1");

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }

        [Test]
        public void Target_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("qwe", "asd", "synonym_test_synonym_1");
            var expectedTargetName = new Identifier(database.ServerName, "qwe", "asd", "synonym_test_synonym_1");

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }

        [Test]
        public void Target_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");
            var expectedTargetName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");

            var synonym = new SqlServerDatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }
    }
}
