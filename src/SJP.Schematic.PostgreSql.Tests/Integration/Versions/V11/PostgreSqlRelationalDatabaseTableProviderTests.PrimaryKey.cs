using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql11Test
    {
        [Test]
        public async Task PrimaryKey_WhenGivenTableWithNoPrimaryKey_ReturnsNone()
        {
            var table = await GetTableAsync("v11_table_test_table_1").ConfigureAwait(false);

            Assert.That(table.PrimaryKey, OptionIs.None);
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = await GetTableAsync("v11_table_test_table_2").ConfigureAwait(false);
            var keyType = table.PrimaryKey.UnwrapSome().KeyType;

            Assert.That(keyType, Is.EqualTo(DatabaseKeyType.Primary));
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = await GetTableAsync("v11_table_test_table_2").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.That(pkColumns, Has.Exactly(1).Items);
                Assert.That(pkColumns.Single().Name.LocalName, Is.EqualTo("test_column"));
            });
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = await GetTableAsync("v11_table_test_table_3").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();
            var pkColumns = pk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.That(pkColumns, Has.Exactly(1).Items);
                Assert.That(pkColumns.Single().Name.LocalName, Is.EqualTo("test_column"));
            });
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithSingleColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = await GetTableAsync("v11_table_test_table_3").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();

            Assert.That(pk.Name.UnwrapSome().LocalName, Is.EqualTo("pk_test_table_3"));
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = await GetTableAsync("v11_table_test_table_4").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();
            var pkColumns = pk.Columns.ToList();
            var pkColumnNames = pkColumns.ConvertAll(c => c.Name.LocalName);

            Assert.Multiple(() =>
            {
                Assert.That(pkColumns, Has.Exactly(3).Items);
                Assert.That(pkColumnNames, Is.EqualTo(expectedColumnNames));
            });
        }

        [Test]
        public async Task PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = await GetTableAsync("v11_table_test_table_4").ConfigureAwait(false);
            var pk = table.PrimaryKey.UnwrapSome();

            Assert.That(pk.Name.UnwrapSome().LocalName, Is.EqualTo("pk_test_table_4"));
        }
    }
}