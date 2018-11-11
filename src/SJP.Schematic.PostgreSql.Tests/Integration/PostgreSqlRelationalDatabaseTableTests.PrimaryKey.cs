using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal partial class PostgreSqlRelationalDatabaseTableTests : PostgreSqlTest
    {
        [Test]
        public void PrimaryKey_WhenGivenTableWithNoPrimaryKey_ReturnsNull()
        {
            var table = Database.GetTable("table_test_table_1").UnwrapSome();

            Assert.IsNull(table.PrimaryKey);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = Database.GetTable("table_test_table_2").UnwrapSome();

            Assert.AreEqual(DatabaseKeyType.Primary, table.PrimaryKey.KeyType);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_2").UnwrapSome();
            var pk = table.PrimaryKey;
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = Database.GetTable("table_test_table_3").UnwrapSome();
            var pk = table.PrimaryKey;
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_3").UnwrapSome();
            var pk = table.PrimaryKey;

            Assert.AreEqual("pk_test_table_3", pk.Name.LocalName);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = Database.GetTable("table_test_table_4").UnwrapSome();
            var pk = table.PrimaryKey;
            var pkColumns = pk.Columns.ToList();

            var columnsEqual = pkColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, pkColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = Database.GetTable("table_test_table_4").UnwrapSome();
            var pk = table.PrimaryKey;

            Assert.AreEqual("pk_test_table_4", pk.Name.LocalName);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithNoPrimaryKey_ReturnsNull()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var pk = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);

            Assert.IsNull(pk);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var primaryKey = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);

            Assert.AreEqual(DatabaseKeyType.Primary, primaryKey.KeyType);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var pk = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_3").ConfigureAwait(false);
            var pk = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_3").ConfigureAwait(false);
            var pk = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);

            Assert.AreEqual("pk_test_table_3", pk.Name.LocalName);
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var tableOption = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var pk = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);
            var pkColumns = pk.Columns.ToList();

            var columnsEqual = pkColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, pkColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task PrimaryKeyAsync_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var pk = await tableOption.UnwrapSome().PrimaryKeyAsync().ConfigureAwait(false);

            Assert.AreEqual("pk_test_table_4", pk.Name.LocalName);
        }
    }
}