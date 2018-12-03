using System;
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
        public void GetSynonym_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SynonymProvider.GetSynonym(null));
        }

        [Test]
        public void GetSynonym_WhenSynonymPresent_ReturnsSynonym()
        {
            var synonym = SynonymProvider.GetSynonym("db_test_synonym_1");
            Assert.IsTrue(synonym.IsSome);
        }

        [Test]
        public void GetSynonym_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            var synonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var synonym = SynonymProvider.GetSynonym(synonymName).UnwrapSome();

            Assert.AreEqual(synonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymExistsGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = SynonymProvider.GetSynonym(synonymName).UnwrapSome();

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymExistsGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = SynonymProvider.GetSynonym(synonymName).UnwrapSome();

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymExistsGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = SynonymProvider.GetSynonym(synonymName).UnwrapSome();

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymExistsGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = SynonymProvider.GetSynonym(synonymName).UnwrapSome();

            Assert.AreEqual(synonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonymOption = SynonymProvider.GetSynonym(synonymName);
            var synonym = synonymOption.UnwrapSome();

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonymOption = SynonymProvider.GetSynonym(synonymName);
            var synonym = synonymOption.UnwrapSome();

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public void GetSynonym_WhenSynonymMissing_ReturnsNone()
        {
            var synonym = SynonymProvider.GetSynonym("synonym_that_doesnt_exist");
            Assert.IsTrue(synonym.IsNone);
        }

        [Test]
        public void GetSynonym_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_synonym_1");
            var synonym = SynonymProvider.GetSynonym(inputName).UnwrapSome();

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonym.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public void GetSynonym_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "DB_TEST_synonym_1");
            var synonym = SynonymProvider.GetSynonym(inputName).UnwrapSome();

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonym.Name.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonym.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public void GetSynonymAsync_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SynonymProvider.GetSynonymAsync(null));
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymPresent_ReturnsSynonym()
        {
            var synonymIsSome = await SynonymProvider.GetSynonymAsync("db_test_synonym_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(synonymIsSome);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
        {
            const string synonymName = "db_test_synonym_1";
            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(synonymName, synonym.Name.LocalName);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymExistsGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymExistsGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymExistsGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymExistsGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(synonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var synonymName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_synonym_1");
            var expectedSynonymName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_synonym_1");

            var synonym = await SynonymProvider.GetSynonymAsync(synonymName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymMissing_ReturnsNone()
        {
            var synonymIsNone = await SynonymProvider.GetSynonymAsync("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(synonymIsNone);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("DB_TEST_synonym_1");
            var synonym = await SynonymProvider.GetSynonymAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, synonym.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetSynonymAsync_WhenSynonymPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "DB_TEST_synonym_1");
            var synonym = await SynonymProvider.GetSynonymAsync(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, synonym.Name.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, synonym.Name.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public void Synonyms_WhenEnumerated_ContainsSynonyms()
        {
            var synonyms = SynonymProvider.Synonyms.ToList();

            Assert.NotZero(synonyms.Count);
        }

        [Test]
        public void Synonyms_WhenEnumerated_ContainsTestSynonym()
        {
            var containsTestSynonym = SynonymProvider.Synonyms.Any(s => s.Name.LocalName == "db_test_synonym_1");

            Assert.True(containsTestSynonym);
        }

        [Test]
        public async Task SynonymsAsync_WhenEnumerated_ContainsSynonyms()
        {
            var synonyms = await SynonymProvider.SynonymsAsync().ConfigureAwait(false);

            Assert.NotZero(synonyms.Count);
        }

        [Test]
        public async Task SynonymsAsync_WhenEnumerated_ContainsTestSynonym()
        {
            var synonyms = await SynonymProvider.SynonymsAsync().ConfigureAwait(false);
            var containsTestSynonym = synonyms.Any(s => s.Name.LocalName == "db_test_synonym_1");

            Assert.IsTrue(containsTestSynonym);
        }

        [Test]
        public void GetSynonym_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
        {
            const string expectedTarget = "synonym_test_view_1";
            var synonym = SynonymProvider.GetSynonym("synonym_test_synonym_1").UnwrapSome();

            Assert.AreEqual(expectedTarget, synonym.Target.LocalName);
        }

        [Test]
        public void GetSynonym_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
        {
            const string expectedTarget = "synonym_test_table_1";
            var synonym = SynonymProvider.GetSynonym("synonym_test_synonym_2").UnwrapSome();

            Assert.AreEqual(expectedTarget, synonym.Target.LocalName);
        }

        [Test]
        public void GetSynonym_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "non_existent_target");
            var synonym = SynonymProvider.GetSynonym("synonym_test_synonym_3").UnwrapSome();

            Assert.AreEqual(expectedTarget, synonym.Target);
        }

        [Test]
        public async Task GetSynonymAsync_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_test_view_1");
            var synonym = await SynonymProvider.GetSynonymAsync("synonym_test_synonym_1").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTarget, synonym.Target);
        }

        [Test]
        public async Task GetSynonymAsync_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "synonym_test_table_1");
            var synonym = await SynonymProvider.GetSynonymAsync("synonym_test_synonym_2").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTarget, synonym.Target);
        }

        [Test]
        public async Task GetSynonymAsync_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
        {
            var expectedTarget = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "non_existent_target");
            var synonym = await SynonymProvider.GetSynonymAsync("synonym_test_synonym_3").UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTarget, synonym.Target);
        }
    }
}
