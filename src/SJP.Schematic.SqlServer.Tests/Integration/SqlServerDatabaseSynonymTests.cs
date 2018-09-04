using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal class SqlServerDatabaseSynonymTests : SqlServerTest
    {
        public SqlServerDatabaseSynonymTests()
        {
            Database = new SqlServerRelationalDatabase(Dialect, Connection);
        }

        private IRelationalDatabase Database { get; }

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
