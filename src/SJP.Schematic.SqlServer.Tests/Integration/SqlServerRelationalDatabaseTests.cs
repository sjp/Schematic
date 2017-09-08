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
    internal class SqlServerRelationalDatabaseTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual(Database.DefaultSchema, "dbo");
        }

        [TestFixture]
        internal class TableTests : SqlServerTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create table db_test_table_1 ( title nvarchar(200) )").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop table db_test_table_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void TableExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.TableExists(null));
            }

            [Test]
            public void TableExists_WhenTablePresent_ReturnsTrue()
            {
                var tableExists = Database.TableExists("db_test_table_1");
                Assert.IsTrue(tableExists);
            }

            [Test]
            public void TableExists_WhenTableMissing_ReturnsFalse()
            {
                var tableExists = Database.TableExists("table_that_doesnt_exist");
                Assert.IsFalse(tableExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public void TableExists_WhenTablePresentWithDifferentCase_ReturnsTrue()
            {
                var tableExists = Database.TableExists("DB_TEST_table_1");
                Assert.IsTrue(tableExists);
            }

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
                Identifier tableName = "db_test_table_1";
                var table = Database.GetTable(tableName);

                Assert.AreEqual(tableName, table.Name);
            }

            [Test]
            public void GetTable_WhenTableMissing_ReturnsNull()
            {
                var table = Database.GetTable("table_that_doesnt_exist");
                Assert.IsNull(table);
            }

            [Test]
            public void TableExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.TableExistsAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task TableExistsAsync_WhenTablePresent_ReturnsTrue()
            {
                var tableExists = await Database.TableExistsAsync("db_test_table_1").ConfigureAwait(false);
                Assert.IsTrue(tableExists);
            }

            [Test]
            public async Task TableExistsAsync_WhenTableMissing_ReturnsFalse()
            {
                var tableExists = await Database.TableExistsAsync("table_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsFalse(tableExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public async Task TableExistsAsync_WhenTablePresentWithDifferentCase_ReturnsTrue()
            {
                var tableExists = await Database.TableExistsAsync("DB_TEST_table_1").ConfigureAwait(false);
                Assert.IsTrue(tableExists);
            }

            [Test]
            public void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetTableAsync(null).ConfigureAwait(false));
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
                Identifier tableName = "db_test_table_1";
                var table = await Database.GetTableAsync(tableName).ConfigureAwait(false);

                Assert.AreEqual(tableName, table.Name);
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
                var tables = Database.Tables.ToList();

                Assert.NotZero(tables.Count);
            }

            [Test]
            public void Tables_WhenEnumerated_ContainsTestTable()
            {
                var containsTestTable = Database.Tables.Any(t => t.Name == "db_test_table_1");

                Assert.True(containsTestTable);
            }

            [Test]
            public async Task TablesAsync_WhenSubscribed_ContainsTables()
            {
                var tables = await Database.TablesAsync().ToList();

                Assert.NotZero(tables.Count);
            }

            [Test]
            public async Task TablesAsync_WhenSubscribed_ContainsTestTable()
            {
                var tables = await Database.TablesAsync().ToList();
                var containsTestTable = tables.Any(t => t.Name == "db_test_table_1");

                Assert.True(containsTestTable);
            }
        }

        [TestFixture]
        internal class ViewTests : SqlServerTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop view db_test_view_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void ViewExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.ViewExists(null));
            }

            [Test]
            public void ViewExists_WhenViewPresent_ReturnsTrue()
            {
                var viewExists = Database.ViewExists("db_test_view_1");
                Assert.IsTrue(viewExists);
            }

            [Test]
            public void ViewExists_WhenViewMissing_ReturnsFalse()
            {
                var viewExists = Database.ViewExists("view_that_doesnt_exist");
                Assert.IsFalse(viewExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public void ViewExists_WhenViewPresentWithDifferentCase_ReturnsTrue()
            {
                var viewExists = Database.ViewExists("DB_TEST_view_1");
                Assert.IsTrue(viewExists);
            }

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
                var viewName = new Identifier(Database.DefaultSchema, "db_test_view_1");
                var view = Database.GetView(viewName);

                Assert.AreEqual(viewName, view.Name);
            }

            [Test]
            public void GetView_WhenViewMissing_ReturnsNull()
            {
                var view = Database.GetView("view_that_doesnt_exist");
                Assert.IsNull(view);
            }

            [Test]
            public void ViewExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.ViewExistsAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task ViewExistsAsync_WhenViewPresent_ReturnsTrue()
            {
                var viewExists = await Database.ViewExistsAsync("db_test_view_1").ConfigureAwait(false);
                Assert.IsTrue(viewExists);
            }

            [Test]
            public async Task ViewExistsAsync_WhenViewMissing_ReturnsFalse()
            {
                var viewExists = await Database.ViewExistsAsync("view_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsFalse(viewExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public async Task ViewExistsAsync_WhenViewPresentWithDifferentCase_ReturnsTrue()
            {
                var viewExists = await Database.ViewExistsAsync("DB_TEST_view_1").ConfigureAwait(false);
                Assert.IsTrue(viewExists);
            }

            [Test]
            public void GetViewAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetViewAsync(null).ConfigureAwait(false));
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
                var viewName = new Identifier(Database.DefaultSchema, "db_test_view_1");
                var view = await Database.GetViewAsync(viewName).ConfigureAwait(false);

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
                var views = Database.Views.ToList();

                Assert.NotZero(views.Count);
            }

            [Test]
            public void Views_WhenEnumerated_ContainsTestView()
            {
                var viewName = new Identifier(Database.DefaultSchema, "db_test_view_1");
                var containsTestView = Database.Views.Any(v => v.Name == viewName);

                Assert.True(containsTestView);
            }

            [Test]
            public async Task ViewsAsync_WhenSubscribed_ContainsViews()
            {
                var views = await Database.ViewsAsync().ToList();

                Assert.NotZero(views.Count);
            }

            [Test]
            public async Task ViewsAsync_WhenSubscribed_ContainsTestView()
            {
                var viewName = new Identifier(Database.DefaultSchema, "db_test_view_1");
                var views = await Database.ViewsAsync().ToList();
                var containsTestView = views.Any(v => v.Name == viewName);

                Assert.True(containsTestView);
            }
        }

        [TestFixture]
        internal class SequenceTests : SqlServerTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create sequence db_test_sequence_1 start with 1000 increment by 1").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop sequence db_test_sequence_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void SequenceExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.SequenceExists(null));
            }

            [Test]
            public void SequenceExists_WhenSequencePresent_ReturnsTrue()
            {
                var sequenceExists = Database.SequenceExists("db_test_sequence_1");
                Assert.IsTrue(sequenceExists);
            }

            [Test]
            public void SequenceExists_WhenSequenceMissing_ReturnsFalse()
            {
                var sequenceExists = Database.SequenceExists("sequence_that_doesnt_exist");
                Assert.IsFalse(sequenceExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public void SequenceExists_WhenSequencePresentWithDifferentCase_ReturnsTrue()
            {
                var sequenceExists = Database.SequenceExists("DB_TEST_sequence_1");
                Assert.IsTrue(sequenceExists);
            }

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
                Identifier sequenceName = "db_test_sequence_1";
                var sequence = Database.GetSequence(sequenceName);

                Assert.AreEqual(sequenceName, sequence.Name);
            }

            [Test]
            public void GetSequence_WhenSequenceMissing_ReturnsNull()
            {
                var sequence = Database.GetSequence("sequence_that_doesnt_exist");
                Assert.IsNull(sequence);
            }

            [Test]
            public void SequenceExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.SequenceExistsAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task SequenceExistsAsync_WhenSequencePresent_ReturnsTrue()
            {
                var sequenceExists = await Database.SequenceExistsAsync("db_test_sequence_1").ConfigureAwait(false);
                Assert.IsTrue(sequenceExists);
            }

            [Test]
            public async Task SequenceExistsAsync_WhenSequenceMissing_ReturnsFalse()
            {
                var sequenceExists = await Database.SequenceExistsAsync("sequence_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsFalse(sequenceExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public async Task SequenceExistsAsync_WhenSequencePresentWithDifferentCase_ReturnsTrue()
            {
                var sequenceExists = await Database.SequenceExistsAsync("DB_TEST_sequence_1").ConfigureAwait(false);
                Assert.IsTrue(sequenceExists);
            }

            [Test]
            public void GetSequenceAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetSequenceAsync(null).ConfigureAwait(false));
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
                Identifier sequenceName = "db_test_sequence_1";
                var sequence = await Database.GetSequenceAsync(sequenceName).ConfigureAwait(false);

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
                var sequences = Database.Sequences.ToList();

                Assert.NotZero(sequences.Count);
            }

            [Test]
            public void Sequences_WhenEnumerated_ContainsTestSequence()
            {
                var containsTestSequence = Database.Sequences.Any(s => s.Name == "db_test_sequence_1");

                Assert.True(containsTestSequence);
            }

            [Test]
            public async Task SequencesAsync_WhenSubscribed_ContainsSequences()
            {
                var sequences = await Database.SequencesAsync().ToList();

                Assert.NotZero(sequences.Count);
            }

            [Test]
            public async Task SequencesAsync_WhenSubscribed_ContainsTestSequence()
            {
                var sequences = await Database.SequencesAsync().ToList();
                var containsTestSequence = sequences.Any(s => s.Name == "db_test_sequence_1");

                Assert.True(containsTestSequence);
            }
        }

        [TestFixture]
        internal class SynonymTests : SqlServerTest
        {
            [OneTimeSetUp]
            public async Task Init()
            {
                await Connection.ExecuteAsync("create synonym db_test_synonym_1 for sys.tables").ConfigureAwait(false);
            }

            [OneTimeTearDown]
            public async Task CleanUp()
            {
                await Connection.ExecuteAsync("drop synonym db_test_synonym_1").ConfigureAwait(false);
            }

            private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

            [Test]
            public void SynonymExists_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.SynonymExists(null));
            }

            [Test]
            public void SynonymExists_WhenSynonymPresent_ReturnsTrue()
            {
                var synonymExists = Database.SynonymExists("db_test_synonym_1");
                Assert.IsTrue(synonymExists);
            }

            [Test]
            public void SynonymExists_WhenSynonymMissing_ReturnsFalse()
            {
                var synonymExists = Database.SynonymExists("synonym_that_doesnt_exist");
                Assert.IsFalse(synonymExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public void SynonymExists_WhenSynonymPresentWithDifferentCase_ReturnsTrue()
            {
                var synonymExists = Database.SynonymExists("DB_TEST_synonym_1");
                Assert.IsTrue(synonymExists);
            }

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
                Identifier synonymName = "db_test_synonym_1";
                var synonym = Database.GetSynonym(synonymName);

                Assert.AreEqual(synonymName, synonym.Name);
            }

            [Test]
            public void GetSynonym_WhenSynonymMissing_ReturnsNull()
            {
                var synonym = Database.GetSynonym("synonym_that_doesnt_exist");
                Assert.IsNull(synonym);
            }

            [Test]
            public void SynonymExistsAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.SynonymExistsAsync(null).ConfigureAwait(false));
            }

            [Test]
            public async Task SynonymExistsAsync_WhenSynonymPresent_ReturnsTrue()
            {
                var synonymExists = await Database.SynonymExistsAsync("db_test_synonym_1").ConfigureAwait(false);
                Assert.IsTrue(synonymExists);
            }

            [Test]
            public async Task SynonymExistsAsync_WhenSynonymMissing_ReturnsFalse()
            {
                var synonymExists = await Database.SynonymExistsAsync("synonym_that_doesnt_exist").ConfigureAwait(false);
                Assert.IsFalse(synonymExists);
            }

            // TODO: This will be dependent on the collation of the database
            //       so may break on a different one
            [Test]
            public async Task SynonymExistsAsync_WhenSynonymPresentWithDifferentCase_ReturnsTrue()
            {
                var synonymExists = await Database.SynonymExistsAsync("DB_TEST_synonym_1").ConfigureAwait(false);
                Assert.IsTrue(synonymExists);
            }

            [Test]
            public void GetSynonymAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.ThrowsAsync<ArgumentNullException>(async () => await Database.GetSynonymAsync(null).ConfigureAwait(false));
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
                Identifier synonymName = "db_test_synonym_1";
                var synonym = await Database.GetSynonymAsync(synonymName).ConfigureAwait(false);

                Assert.AreEqual(synonymName, synonym.Name);
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
                var synonyms = Database.Synonyms.ToList();

                Assert.NotZero(synonyms.Count);
            }

            [Test]
            public void Synonyms_WhenEnumerated_ContainsTestSynonym()
            {
                var containsTestSynonym = Database.Synonyms.Any(s => s.Name == "db_test_synonym_1");

                Assert.True(containsTestSynonym);
            }

            [Test]
            public async Task SynonymsAsync_WhenSubscribed_ContainsSynonyms()
            {
                var synonyms = await Database.SynonymsAsync().ToList();

                Assert.NotZero(synonyms.Count);
            }

            [Test]
            public async Task SynonymsAsync_WhenSubscribed_ContainsTestSynonym()
            {
                var synonyms = await Database.SynonymsAsync().ToList();
                var containsTestSynonym = synonyms.Any(s => s.Name == "db_test_synonym_1");

                Assert.True(containsTestSynonym);
            }
        }
    }
}
