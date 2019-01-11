using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseSynonymProviderTests : OracleTest
    {
        private IDatabaseSynonymProvider SynonymProvider => new OracleDatabaseSynonymProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create synonym db_test_synonym_1 for sys.user_tables").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view synonym_test_view_1 as select 1 as test from dual").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table synonym_test_table_1 (table_id number primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create synonym synonym_test_synonym_1 for synonym_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create synonym synonym_test_synonym_2 for synonym_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create synonym synonym_test_synonym_3 for non_existent_target").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop synonym db_test_synonym_1").ConfigureAwait(false);

            await Connection.ExecuteAsync("drop view synonym_test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table synonym_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop synonym synonym_test_synonym_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop synonym synonym_test_synonym_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop synonym synonym_test_synonym_3").ConfigureAwait(false);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresent_ReturnsSynonym()
        {
            var synonymIsSome = await SynonymProvider.GetSynonym("db_test_synonym_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(synonymIsSome);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            const string synonymName = "db_test_synonym_1";
            const string expectedSynonymName = "DB_TEST_SYNONYM_1";
            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name.LocalName);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(synonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymMissing_ReturnsNone()
        {
            var synonymIsNone = await SynonymProvider.GetSynonym("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(synonymIsNone);
        }

        [Test]
        public async Task GetAllSynonyms_WhenEnumerated_ContainsSynonyms()
        {
            var synonyms = await SynonymProvider.GetAllSynonyms().ConfigureAwait(false);

            Assert.NotZero(synonyms.Count);
        }

        [Test]
        public async Task GetAllSynonyms_WhenEnumerated_ContainsTestSynonym()
        {
            const string expectedSynonymName = "DB_TEST_SYNONYM_1";
            var synonyms = await SynonymProvider.GetAllSynonyms().ConfigureAwait(false);
            var containsTestSynonym = synonyms.Any(s => s.Name.LocalName == expectedSynonymName);

            Assert.IsTrue(containsTestSynonym);
        }

        [Test]
        public async Task GetSynonym_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "SYNONYM_TEST_VIEW_1");
            var synonym = await SynonymProvider.GetSynonym("SYNONYM_TEST_SYNONYM_1").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTarget, synonym.Target);
        }

        [Test]
        public async Task GetSynonym_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "SYNONYM_TEST_TABLE_1");
            var synonym = await SynonymProvider.GetSynonym("SYNONYM_TEST_SYNONYM_2").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTarget, synonym.Target);
        }

        [Test]
        public async Task GetSynonym_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "NON_EXISTENT_TARGET");
            var synonym = await SynonymProvider.GetSynonym("SYNONYM_TEST_SYNONYM_3").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTarget, synonym.Target);
        }
    }
}
