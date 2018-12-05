using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
    {
        [Test]
        public void PrimaryKey_WhenGivenTableWithNoPrimaryKey_ReturnsNone()
        {
            var table = GetTable("table_test_table_1");
            var pkIsNone = table.PrimaryKey.IsNone;

            Assert.IsTrue(pkIsNone);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithPrimaryKey_ReturnsCorrectKeyType()
        {
            var table = GetTable("table_test_table_2");
            var keyType = table.PrimaryKey.UnwrapSome().KeyType;

            Assert.AreEqual(DatabaseKeyType.Primary, keyType);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithColumnAsPrimaryKey_ReturnsPrimaryKeyWithColumnOnly()
        {
            var table = GetTable("table_test_table_2");
            var pk = table.PrimaryKey.UnwrapSome();
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
            var table = GetTable("table_test_table_3");
            var pk = table.PrimaryKey.UnwrapSome();
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
            var table = GetTable("table_test_table_3");
            var pk = table.PrimaryKey.UnwrapSome();

            Assert.AreEqual("pk_test_table_3", pk.Name.LocalName);
        }

        [Test]
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = GetTable("table_test_table_4");
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
        public void PrimaryKey_WhenGivenTableWithMultiColumnConstraintAsPrimaryKey_ReturnsPrimaryKeyWithCorrectName()
        {
            var table = GetTable("table_test_table_4");
            var pk = table.PrimaryKey.UnwrapSome();

            Assert.AreEqual("pk_test_table_4", pk.Name.LocalName);
        }
    }
}