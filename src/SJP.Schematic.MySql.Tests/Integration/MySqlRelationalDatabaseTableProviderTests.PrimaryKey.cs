using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
    {
        [Test]
        public async Task PrimaryKey_WhenGivenTableWithNoPrimaryKey_ReturnsNone()
        {
            var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);
            var pkIsNone = table.PrimaryKey.IsNone;

            Assert.IsTrue(pkIsNone);
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = await GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var keyType = table.PrimaryKey.UnwrapSome().KeyType;

            Assert.AreEqual(DatabaseKeyType.Primary, keyType);
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = await GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = await GetTableAsync("table_test_table_3").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, pkColumns.Count);
                Assert.AreEqual("test_column", pkColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_3").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();

            Assert.AreEqual("PRIMARY", pk.Name.UnwrapSome().LocalName);
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();
            var pkColumns = pk.Columns.ToList();

            var columnsEqual = pkColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, pkColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = await GetTableAsync("table_test_table_4").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();

            Assert.AreEqual("PRIMARY", pk.Name.UnwrapSome().LocalName);
        }
    }
}