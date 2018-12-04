using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
    {
        [Test]
        public void PrimaryKey_WhenGivenTableWithNoPrimaryKey_ReturnsNull()
        {
            var table = TableProvider.GetTable("table_test_table_1").UnwrapSome();

            Assert.IsNull(table.PrimaryKey);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = TableProvider.GetTable("table_test_table_2").UnwrapSome();

            Assert.AreEqual(DatabaseKeyType.Primary, table.PrimaryKey.KeyType);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = TableProvider.GetTable("table_test_table_2").UnwrapSome();
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
            var table = TableProvider.GetTable("table_test_table_3").UnwrapSome();
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
            var table = TableProvider.GetTable("table_test_table_3").UnwrapSome();
            var pk = table.PrimaryKey;

            Assert.AreEqual("pk_test_table_3", pk.Name.LocalName);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = TableProvider.GetTable("table_test_table_4").UnwrapSome();
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
            var table = TableProvider.GetTable("table_test_table_4").UnwrapSome();
            var pk = table.PrimaryKey;

            Assert.AreEqual("pk_test_table_4", pk.Name.LocalName);
        }
    }
}