using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal partial class OracleRelationalDatabaseTableTests : OracleTest
    {
        [Test]
        public void ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_2").UnwrapSome();
            var count = table.ChildKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var count = childKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("PK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("PK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

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
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

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
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_24");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_24");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_25");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_25");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_30");

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_30");

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("UK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("UK_TEST_TABLE_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

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
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

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
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_27");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_27");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_28");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_28");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_31");

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var tableOption = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await tableOption.UnwrapSome().ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_31");

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }
    }
}