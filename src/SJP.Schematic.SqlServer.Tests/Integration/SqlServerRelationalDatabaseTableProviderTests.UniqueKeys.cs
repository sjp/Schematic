using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
    {
        [Test]
        public void UniqueKeys_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
        {
            var table = GetTable("table_test_table_1");
            var count = table.UniqueKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
        {
            var table = GetTable("table_test_table_5");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual(DatabaseKeyType.Unique, uk.KeyType);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = GetTable("table_test_table_5");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
        {
            var table = GetTable("table_test_table_6");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, ukColumns.Count);
                Assert.AreEqual("test_column", ukColumns.Single().Name.LocalName);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = GetTable("table_test_table_6");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("uk_test_table_6", uk.Name.LocalName);
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
        {
            var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

            var table = GetTable("table_test_table_7");
            var uk = table.UniqueKeys.Single();
            var ukColumns = uk.Columns.ToList();

            var columnsEqual = ukColumns.Select(c => c.Name.LocalName).SequenceEqual(expectedColumnNames);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, ukColumns.Count);
                Assert.IsTrue(columnsEqual);
            });
        }

        [Test]
        public void UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
        {
            var table = GetTable("table_test_table_7");
            var uk = table.UniqueKeys.Single();

            Assert.AreEqual("uk_test_table_7", uk.Name.LocalName);
        }
    }
}