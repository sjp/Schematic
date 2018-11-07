using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleRelationalDatabaseTests : OracleTest
    {
        private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection);

        [Test]
        public void Database_PropertyGet_ShouldNotBeEmpty()
        {
            var isEmpty = string.IsNullOrWhiteSpace(Database.DatabaseName);
            Assert.IsFalse(isEmpty);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            // FIXME this could change when connected to a different database
            Assert.AreEqual("SYSTEM", Database.DefaultSchema);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonNull()
        {
            Assert.IsNotNull(Database.DatabaseVersion);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonEmpty()
        {
            Assert.AreNotEqual(string.Empty, Database.DatabaseVersion);
        }

        internal sealed class TableTests : OracleTest
        {
            [OneTimeSetUp]
            public Task Init()
            {
                return Connection.ExecuteAsync("create table db_test_table_1 ( title varchar2(200) )");
            }

            [OneTimeTearDown]
            public Task CleanUp()
            {
                return Connection.ExecuteAsync("drop table db_test_table_1");
            }

            private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetTable_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetTable(null));
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTable()
            {
                var table = Database.GetTable("db_test_table_1");
                Assert.NotNull(table);
            }

            [Test]
            public void GetTable_WhenTablePresent_ReturnsTableWithCorrectName()
            {
                const string tableName = "db_test_table_1";
                const string expectedTableName = "DB_TEST_TABLE_1";
                var table = Database.GetTable(tableName);

                Assert.AreEqual(expectedTableName, table.Name.LocalName);
            }

            [Test]
            public void GetTable_WhenTableMissing_ReturnsNull()
            {
                var table = Database.GetTable("table_that_doesnt_exist");
                Assert.IsNull(table);
            }

            [Test]
            public void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetTableAsync(null));
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresent_ReturnsTable()
            {
                var table = await Database.GetTableAsync("db_test_table_1").ConfigureAwait(false);
                Assert.NotNull(table);
            }

            [Test]
            public async Task GetTableAsync_WhenTablePresent_ReturnsTableWithCorrectName()
            {
                const string tableName = "db_test_table_1";
                const string expectedTableName = "DB_TEST_TABLE_1";
                var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);

                Assert.AreEqual(expectedTableName, table.Name.LocalName);
            }

            [Test]
            public async Task GetTableAsync_WhenTableMissing_ReturnsNull()
            {
                var table = await Database.GetTableAsync("table_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(table);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTables()
            {
                var tables = Database.Tables;

                Assert.NotZero(tables.Count);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTestTable()
            {
                const string expectedTableName = "DB_TEST_TABLE_1";
                var containsTestTable = Database.Tables.Any(t => t.Name.LocalName == expectedTableName);

                Assert.True(containsTestTable);
            }

            [Test]
            public async Task TablesAsync_WhenSubscribed_ContainsTables()
            {
                var tables = await Database.TablesAsync().ConfigureAwait(false);

                Assert.NotZero(tables.Count);
            }

            [Test]
            public async Task TablesAsync_WhenSubscribed_ContainsTestTable()
            {
                const string expectedTableName = "DB_TEST_TABLE_1";
                var tableCollection = await Database.TablesAsync().ConfigureAwait(false);
                var tables = await Task.WhenAll(tableCollection).ConfigureAwait(false);
                var containsTestTable = tables.Any(t => t.Name.LocalName == expectedTableName);

                Assert.True(containsTestTable);
            }
        }

        internal sealed class ViewTests : OracleTest
        {
            [OneTimeSetUp]
            public Task Init()
            {
                return Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy from dual");
            }

            [OneTimeTearDown]
            public Task CleanUp()
            {
                return Connection.ExecuteAsync("drop view db_test_view_1");
            }

            private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetView_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetView(null));
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsView()
            {
                var view = Database.GetView("db_test_view_1");
                Assert.NotNull(view);
            }

            [Test]
            public void GetView_WhenViewPresent_ReturnsViewWithCorrectName()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");
                var view = database.GetView(viewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("DB_TEST_VIEW_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = database.GetView(viewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.DefaultSchema, "DB_TEST_VIEW_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = database.GetView(viewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = database.GetView(viewName);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = database.GetView(viewName);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewMissing_ReturnsNull()
            {
                var view = Database.GetView("view_that_doesnt_exist");
                Assert.IsNull(view);
            }

            [Test]
            public void GetViewAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetViewAsync(null));
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsView()
            {
                var view = await Database.GetViewAsync("db_test_view_1").ConfigureAwait(false);
                Assert.NotNull(view);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresent_ReturnsViewWithCorrectName()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "db_test_view_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");
                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier("DB_TEST_VIEW_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.DefaultSchema, "DB_TEST_VIEW_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");
                var expectedViewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(expectedViewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var viewName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_VIEW_1");

                var view = await database.GetViewAsync(viewName).ConfigureAwait(false);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public async Task GetViewAsync_WhenViewMissing_ReturnsNull()
            {
                var view = await Database.GetViewAsync("view_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(view);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsViews()
            {
                var views = Database.Views;

                Assert.NotZero(views.Count);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsTestView()
            {
                const string viewName = "DB_TEST_VIEW_1";
                var containsTestView = Database.Views.Any(v => v.Name.LocalName == viewName);

                Assert.True(containsTestView);
            }

            [Test]
            public async Task ViewsAsync_WhenSubscribed_ContainsViews()
            {
                var views = await Database.ViewsAsync().ConfigureAwait(false);

                Assert.NotZero(views.Count);
            }

            [Test]
            public async Task ViewsAsync_WhenSubscribed_ContainsTestView()
            {
                const string viewName = "DB_TEST_VIEW_1";
                var viewsCollection = await Database.ViewsAsync().ConfigureAwait(false);
                var views = await Task.WhenAll(viewsCollection).ConfigureAwait(false);
                var containsTestView = views.Any(v => v.Name.LocalName == viewName);

                Assert.True(containsTestView);
            }
        }

        internal sealed class SequenceTests : OracleTest
        {
            [OneTimeSetUp]
            public Task Init()
            {
                return Connection.ExecuteAsync("create sequence db_test_sequence_1 start with 1000 increment by 1");
            }

            [OneTimeTearDown]
            public Task CleanUp()
            {
                return Connection.ExecuteAsync("drop sequence db_test_sequence_1");
            }

            private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSequence_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public void GetSequence_WhenSequencePresent_ReturnsSequence()
            {
                var sequence = Database.GetSequence("db_test_sequence_1");
                Assert.NotNull(sequence);
            }

            [Test]
            public void GetSequence_WhenSequencePresent_ReturnsSequenceWithCorrectName()
            {
                const string sequenceName = "db_test_sequence_1";
                const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

                var sequence = Database.GetSequence(sequenceName);

                Assert.AreEqual(expectedSequenceName, sequence.Name.LocalName);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("DB_TEST_SEQUENCE_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = database.GetSequence(sequenceName);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DefaultSchema, "DB_TEST_SEQUENCE_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = database.GetSequence(sequenceName);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = database.GetSequence(sequenceName);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = database.GetSequence(sequenceName);

                Assert.AreEqual(sequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequenceMissing_ReturnsNull()
            {
                var sequence = Database.GetSequence("sequence_that_doesnt_exist");
                Assert.IsNull(sequence);
            }

            [Test]
            public void GetSequenceAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(null));
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresent_ReturnsSequence()
            {
                var sequence = await Database.GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);
                Assert.NotNull(sequence);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresent_ReturnsSequenceWithCorrectName()
            {
                const string sequenceName = "db_test_sequence_1";
                const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

                var sequence = await Database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name.LocalName);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier("DB_TEST_SEQUENCE_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = await database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DefaultSchema, "DB_TEST_SEQUENCE_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = await database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");
                var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = await database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.AreEqual(expectedSequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
            {
                var database = Database;
                var sequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "DB_TEST_SEQUENCE_1");

                var sequence = await database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

                Assert.AreEqual(sequenceName, sequence.Name);
            }

            [Test]
            public async Task GetSequenceAsync_WhenSequenceMissing_ReturnsNull()
            {
                var sequence = await Database.GetSequenceAsync("sequence_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(sequence);
            }

            [Test]
            public void Sequences_WhenEnumerated_ContainsSequences()
            {
                var sequences = Database.Sequences;

                Assert.NotZero(sequences.Count);
            }

            [Test]
            public void Sequences_WhenEnumerated_ContainsTestSequence()
            {
                const string expectedSequenceName = "DB_TEST_SEQUENCE_1";
                var containsTestSequence = Database.Sequences.Any(s => s.Name.LocalName == expectedSequenceName);

                Assert.True(containsTestSequence);
            }

            [Test]
            public async Task SequencesAsync_WhenSubscribed_ContainsSequences()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);

                Assert.NotZero(sequences.Count);
            }

            [Test]
            public async Task SequencesAsync_WhenSubscribed_ContainsTestSequence()
            {
                const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

                var sequenceCollection = await Database.SequencesAsync().ConfigureAwait(false);
                var sequences = await Task.WhenAll(sequenceCollection).ConfigureAwait(false);
                var containsTestSequence = sequences.Any(s => s.Name.LocalName == expectedSequenceName);

                Assert.True(containsTestSequence);
            }
        }

        internal sealed class SynonymTests : OracleTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create synonym db_test_synonym_1 for user_tables").ConfigureAwait(false);

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

            private IRelationalDatabase Database => new OracleRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSynonym_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_WhenSynonymPresent_ReturnsSynonym()
            {
                var synonym = Database.GetSynonym("db_test_synonym_1");
                Assert.NotNull(synonym);
            }

            [Test]
            public void GetSynonym_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
            {
                const string expectedSynonymName = "DB_TEST_SYNONYM_1";

                var database = Database;
                var synonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, expectedSynonymName);
                var synonym = database.GetSynonym(synonymName);

                Assert.AreEqual(synonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymMissing_ReturnsNull()
            {
                var synonym = Database.GetSynonym("synonym_that_doesnt_exist");
                Assert.IsNull(synonym);
            }

            [Test]
            public void GetSynonymAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresent_ReturnsSynonym()
            {
                var synonym = await Database.GetSynonymAsync("db_test_synonym_1").ConfigureAwait(false);
                Assert.NotNull(synonym);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymPresent_ReturnsSynonymWithCorrectName()
            {
                var database = Database;
                const string synonymName = "db_test_synonym_1";
                const string expectedSynonymName = "DB_TEST_SYNONYM_1";
                var synonym = await database.GetSynonymAsync(synonymName).ConfigureAwait(false);

                Assert.AreEqual(expectedSynonymName, synonym.Name.LocalName);
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymMissing_ReturnsNull()
            {
                var synonym = await Database.GetSynonymAsync("synonym_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsNull(synonym);
            }

            [Test]
            public void Synonyms_WhenEnumerated_ContainsSynonyms()
            {
                var synonyms = Database.Synonyms;

                Assert.NotZero(synonyms.Count);
            }

            [Test]
            public void Synonyms_WhenEnumerated_ContainsTestSynonym()
            {
                const string expectedSynonymName = "DB_TEST_SYNONYM_1";
                var containsTestSynonym = Database.Synonyms.Any(s => s.Name.LocalName == expectedSynonymName);

                Assert.True(containsTestSynonym);
            }

            [Test]
            public async Task SynonymsAsync_WhenSubscribed_ContainsSynonyms()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

                Assert.NotZero(synonyms.Count);
            }

            [Test]
            public async Task SynonymsAsync_WhenSubscribed_ContainsTestSynonym()
            {
                const string expectedSynonymName = "DB_TEST_SYNONYM_1";
                var synonymCollection = await Database.SynonymsAsync().ConfigureAwait(false);
                var synonyms = await Task.WhenAll(synonymCollection).ConfigureAwait(false);
                var containsTestSynonym = synonyms.Any(s => s.Name.LocalName == expectedSynonymName);

                Assert.True(containsTestSynonym);
            }

            [Test]
            public void GetSynonym_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                const string expectedTarget = "SYNONYM_TEST_VIEW_1";
                var synonym = database.GetSynonym("synonym_test_synonym_1");

                Assert.AreEqual(expectedTarget, synonym.Target.LocalName);
            }

            [Test]
            public void GetSynonym_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                const string expectedTarget = "SYNONYM_TEST_TABLE_1";
                var synonym = database.GetSynonym("synonym_test_synonym_2");

                Assert.AreEqual(expectedTarget, synonym.Target.LocalName);
            }

            [Test]
            public void GetSynonym_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "NON_EXISTENT_TARGET");
                var synonym = database.GetSynonym("SYNONYM_TEST_SYNONYM_3");

                Assert.AreEqual(expectedTarget, synonym.Target);
            }

            [Test]
            public async Task GetSynonymAsync_ForSynonymToView_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "SYNONYM_TEST_VIEW_1");
                var synonym = await database.GetSynonymAsync("synonym_test_synonym_1").ConfigureAwait(false);

                Assert.AreEqual(expectedTarget, synonym.Target);
            }

            [Test]
            public async Task GetSynonymAsync_ForSynonymToTable_ReturnsSynonymWithCorrectTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "SYNONYM_TEST_TABLE_1");
                var synonym = await database.GetSynonymAsync("synonym_test_synonym_2").ConfigureAwait(false);

                Assert.AreEqual(expectedTarget, synonym.Target);
            }

            [Test]
            public async Task GetSynonymAsync_ForSynonymToMissingObject_ReturnsSynonymWithMissingTarget()
            {
                var database = Database;
                var expectedTarget = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "NON_EXISTENT_TARGET");
                var synonym = await database.GetSynonymAsync("synonym_test_synonym_3").ConfigureAwait(false);

                Assert.AreEqual(expectedTarget, synonym.Target);
            }
        }
    }
}
