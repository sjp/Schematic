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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_16");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_24");

            Assert.AreEqual(ReferentialAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_25");

            Assert.AreEqual(ReferentialAction.SetNull, foreignKey.DeleteAction);
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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_17");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_27");

            Assert.AreEqual(ReferentialAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "TABLE_TEST_TABLE_28");

            Assert.AreEqual(ReferentialAction.SetNull, foreignKey.DeleteAction);
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