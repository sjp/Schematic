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
        public async Task ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("table_test_table_2").ConfigureAwait(false);
            var count = table.ChildKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_16", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("PK_TEST_TABLE_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_24");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_25");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_30");

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_17", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("UK_TEST_TABLE_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_27");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_28");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_31");

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }
    }
}