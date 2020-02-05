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

            Assert.That(table.ChildKeys, Is.Empty);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_16"));
                Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("pk_test_table_15"));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
                Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("v94_table_test_table_16"));
                Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("v94_table_test_table_15"));
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

            Assert.Multiple(() =>
            {
                Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
                Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_18");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_19");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateAction_ReturnsUpdateActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_20");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_24");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_25");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteAction_ReturnsDeleteActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_26");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_16");

            Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_17"));
                Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_15"));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
                Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("v94_table_test_table_17"));
                Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("v94_table_test_table_15"));
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

            Assert.Multiple(() =>
            {
                Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
                Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_21");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_22");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateAction_ReturnsUpdateActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_23");

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_27");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_28");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteAction_ReturnsDeleteActionAsSetDefault()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_29");

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v94_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v94_table_test_table_17");

            Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
        }
    }
}