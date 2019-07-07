using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_4
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql94Test
    {
        [Test]
        public async Task ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("v94_table_test_table_2").ConfigureAwait(false);
            var count = table.ChildKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("v94_table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("v94_table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_18");

            Assert.AreEqual(ReferentialAction.Cascade, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_19");

            Assert.AreEqual(ReferentialAction.SetNull, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateAction_ReturnsUpdateActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_20");

            Assert.AreEqual(ReferentialAction.SetDefault, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_24");

            Assert.AreEqual(ReferentialAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_25");

            Assert.AreEqual(ReferentialAction.SetNull, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteAction_ReturnsDeleteActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_26");

            Assert.AreEqual(ReferentialAction.SetDefault, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("v94_table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("v94_table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_21");

            Assert.AreEqual(ReferentialAction.Cascade, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_22");

            Assert.AreEqual(ReferentialAction.SetNull, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateAction_ReturnsUpdateActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_23");

            Assert.AreEqual(ReferentialAction.SetDefault, foreignKey.UpdateAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.AreEqual(ReferentialAction.NoAction, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_27");

            Assert.AreEqual(ReferentialAction.Cascade, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_28");

            Assert.AreEqual(ReferentialAction.SetNull, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteAction_ReturnsDeleteActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_29");

            Assert.AreEqual(ReferentialAction.SetDefault, foreignKey.DeleteAction);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }
    }
}