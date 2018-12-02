using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableProviderTests : OracleTest
    {
        [Test]
        public void ParentKeys_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var count = table.ParentKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_15").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var count = parentKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("PK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("PK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "FIRST_NAME_CHILD" };
            var expectedParentColumns = new[] { "FIRST_NAME_PARENT" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "FIRST_NAME_CHILD" };
            var expectedParentColumns = new[] { "FIRST_NAME_PARENT" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = TableProvider.GetTable("table_test_table_24").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_24").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = TableProvider.GetTable("table_test_table_25").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_25").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = TableProvider.GetTable("table_test_table_16").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_16").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = TableProvider.GetTable("table_test_table_30").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_30").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("UK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("UK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var foreignKey = table.ParentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "LAST_NAME_CHILD", "MIDDLE_NAME_CHILD" };
            var expectedParentColumns = new[] { "LAST_NAME_PARENT", "MIDDLE_NAME_PARENT" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "LAST_NAME_CHILD", "MIDDLE_NAME_CHILD" };
            var expectedParentColumns = new[] { "LAST_NAME_PARENT", "MIDDLE_NAME_PARENT" };

            var childColumnsEqual = childColumns.SequenceEqual(expectedChildColumns);
            var parentColumnsEqual = parentColumns.SequenceEqual(expectedParentColumns);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(childColumnsEqual);
                Assert.IsTrue(parentColumnsEqual);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = TableProvider.GetTable("table_test_table_27").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_27").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = TableProvider.GetTable("table_test_table_28").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_28").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = TableProvider.GetTable("table_test_table_17").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_17").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = TableProvider.GetTable("table_test_table_31").UnwrapSome();
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = await TableProvider.GetTableAsync("table_test_table_31").UnwrapSomeAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }
    }
}