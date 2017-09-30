using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration
{
    [TestFixture]
    internal partial class SqliteRelationalDatabaseTableTests : SqliteTest
    {
        [Test]
        public void ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_2");
            var count = table.ChildKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var count = childKeys.Count();

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "first_name_child" };
            var expectedParentColumns = new[] { "first_name_parent" };

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
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "first_name_child" };
            var expectedParentColumns = new[] { "first_name_parent" };

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
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_18");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_18");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_19");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_19");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_20");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_20");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_24");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_24");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_25");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_25");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_26");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_26");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildKey.Table.Name.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentKey.Table.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_15");
            var foreignKey = table.ChildKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
            var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

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
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
            var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

            var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
            var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

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
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_21");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_21");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_22");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_22");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_23");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_23");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.UpdateAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.AreEqual(RelationalKeyUpdateAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_27");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_27");

            Assert.AreEqual(RelationalKeyUpdateAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_28");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_28");

            Assert.AreEqual(RelationalKeyUpdateAction.SetNull, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_29");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_29");

            Assert.AreEqual(RelationalKeyUpdateAction.SetDefault, foreignKey.DeleteAction);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_15");
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeysAsync_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var foreignKey = childKeys.Single(k => k.ChildKey.Table.Name.LocalName == "table_test_table_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }
    }
}