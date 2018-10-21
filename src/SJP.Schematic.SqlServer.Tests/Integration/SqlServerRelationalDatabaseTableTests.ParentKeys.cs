using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableTests : SqlServerTest
    {
        [Test]
        public void ParentKey_WhenGivenTableWithNoForeignKeys_ReturnsEmptyLookup()
        {
            var table = Database.GetTable("table_test_table_15");
            var parentKeyLookup = table.ParentKey;

            Assert.AreEqual(0, parentKeyLookup.Count);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = Database.GetTable("table_test_table_15");
            var count = table.ParentKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithNoForeignKeys_ReturnsEmptyLookup()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);

            Assert.AreEqual(0, parentKeyLookup.Count);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = await Database.GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var count = parentKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_16"];

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
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_16");
            var foreignKey = table.ParentKeys.Single();

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
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_16"];

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
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

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
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_18");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_18");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_18").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_18").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_19");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_19");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_19").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_19").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_20");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_20");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_20").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_20").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_24");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_24");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_24").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_24").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_25");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_25");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_25").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_25").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_26");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_26");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_26").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_26").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_16");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_30");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_30");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_30").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_30").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup["fk_test_table_17"];

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
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = Database.GetTable("table_test_table_17");
            var foreignKey = table.ParentKeys.Single();

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
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup["fk_test_table_17"];

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
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

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
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_21");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_21");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_21").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_21").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_22");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_22");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_22").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_22").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_23");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_23");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_23").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_23").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_27");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = Database.GetTable("table_test_table_27");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_27").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await Database.GetTableAsync("table_test_table_27").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_28");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = Database.GetTable("table_test_table_28");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_28").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await Database.GetTableAsync("table_test_table_28").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_29");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = Database.GetTable("table_test_table_29");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_29").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await Database.GetTableAsync("table_test_table_29").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = Database.GetTable("table_test_table_17");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await Database.GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKey_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_31");
            var parentKeyLookup = table.ParentKey;
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ParentKeys_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = Database.GetTable("table_test_table_31");
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeyAsync_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_31").ConfigureAwait(false);
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var foreignKey = parentKeyLookup.Values.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeysAsync_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = await Database.GetTableAsync("table_test_table_31").ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }
    }
}