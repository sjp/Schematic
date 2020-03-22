using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseSynonymProviderTests : OracleTest
    {
        private IDatabaseSynonymProvider SynonymProvider => new OracleDatabaseSynonymProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync("create synonym db_test_synonym_1 for sys.user_tables", CancellationToken.None).ConfigureAwait(false);

            await DbConnection.ExecuteAsync("create view synonym_test_view_1 as select 1 as test from dual", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table synonym_test_table_1 (table_id number primary key not null)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create synonym synonym_test_synonym_1 for synonym_test_view_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create synonym synonym_test_synonym_2 for synonym_test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create synonym synonym_test_synonym_3 for non_existent_target", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop synonym db_test_synonym_1", CancellationToken.None).ConfigureAwait(false);

            await DbConnection.ExecuteAsync("drop view synonym_test_view_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table synonym_test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop synonym synonym_test_synonym_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop synonym synonym_test_synonym_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop synonym synonym_test_synonym_3", CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresent_ReturnsSynonym()
        {
            var synonymIsSome = await SynonymProvider.GetSynonym("db_test_synonym_1").IsSome.ConfigureAwait(false);
            Assert.That(synonymIsSome, Is.True);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            const string synonymName = "db_test_synonym_1";
            const string expectedSynonymName = "DB_TEST_SYNONYM_1";
            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name.LocalName, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(synonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SYNONYM_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymMissing_ReturnsNone()
        {
            var synonymIsNone = await SynonymProvider.GetSynonym("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(synonymIsNone, Is.True);
        }

        [Test]
        public async Task GetAllSynonyms_WhenEnumerated_ContainsSynonyms()
        {
            var hasSynonyms = await SynonymProvider.GetAllSynonyms()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasSynonyms, Is.True);
        }

        [Test]
        public async Task GetAllSynonyms_WhenEnumerated_ContainsTestSynonym()
        {
            const string expectedSynonymName = "DB_TEST_SYNONYM_1";
            var containsTestSynonym = await SynonymProvider.GetAllSynonyms()
                .AnyAsync(s => s.Name.LocalName == expectedSynonymName)
                .ConfigureAwait(false);

            Assert.That(containsTestSynonym, Is.True);
        }

        [Test]
        public async Task GetSynonym_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "SYNONYM_TEST_VIEW_1");
            var synonym = await SynonymProvider.GetSynonym("SYNONYM_TEST_SYNONYM_1").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Target, Is.EqualTo(expectedTarget));
        }

        [Test]
        public async Task GetSynonym_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "SYNONYM_TEST_TABLE_1");
            var synonym = await SynonymProvider.GetSynonym("SYNONYM_TEST_SYNONYM_2").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Target, Is.EqualTo(expectedTarget));
        }

        [Test]
        public async Task GetSynonym_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "NON_EXISTENT_TARGET");
            var synonym = await SynonymProvider.GetSynonym("SYNONYM_TEST_SYNONYM_3").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Target, Is.EqualTo(expectedTarget));
        }
    }
}
