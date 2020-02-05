using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal sealed class SqlServerDatabaseSynonymProviderTests : SqlServerTest
    {
        private IDatabaseSynonymProvider SynonymProvider => new SqlServerDatabaseSynonymProvider(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create synonym db_test_synonym_1 for sys.tables").ConfigureAwait(false);

            await Connection.ExecuteAsync("create view synonym_test_view_1 as select 1 as test").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table synonym_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
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
            Assert.That(synonymIsSome, Is.True);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            const string synonymName = "db_test_synonym_1";
            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name.LocalName, Is.EqualTo(synonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymExistsGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymExistsGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymExistsGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymExistsGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(synonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonym(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Name, Is.EqualTo(expectedSynonymName));
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

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
        public async Task GetSynonym_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_synonym_1");
            var synonym = await SynonymProvider.GetSynonym(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonym.Name.LocalName);
            Assert.That(equalNames, Is.True);
        }

        [Test]
        public async Task GetSynonym_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "DB_TEST_synonym_1");
            var synonym = await SynonymProvider.GetSynonym(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonym.Name.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonym.Name.LocalName);
            Assert.That(equalNames, Is.True);
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
            var containsTestSynonym = await SynonymProvider.GetAllSynonyms()
                .AnyAsync(s => s.Name.LocalName == "db_test_synonym_1")
                .ConfigureAwait(false);

            Assert.That(containsTestSynonym, Is.True);
        }

        [Test]
        public async Task GetSynonym_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_test_view_1");
            var synonym = await SynonymProvider.GetSynonym("synonym_test_synonym_1").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Target, Is.EqualTo(expectedTarget));
        }

        [Test]
        public async Task GetSynonym_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_test_table_1");
            var synonym = await SynonymProvider.GetSynonym("synonym_test_synonym_2").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Target, Is.EqualTo(expectedTarget));
        }

        [Test]
        public async Task GetSynonym_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "non_existent_target");
            var synonym = await SynonymProvider.GetSynonym("synonym_test_synonym_3").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(synonym.Target, Is.EqualTo(expectedTarget));
        }
    }
}
