using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11
{
    internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql11Test
    {
        [Test]
        public async Task ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("v11_table_test_table_2").ConfigureAwait(false);

            Assert.That(table.ChildKeys, Is.Empty);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_16"));
                Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("pk_test_table_15"));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
                Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("v11_table_test_table_16"));
                Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("v11_table_test_table_15"));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateReferentialAction_ReturnsUpdateReferentialActionAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_18", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_19", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_20", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteReferentialAction_ReturnsDeleteReferentialActionAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_24", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_25", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_26", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_16", System.StringComparison.Ordinal));

            Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_17"));
                Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_15"));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
                Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("v11_table_test_table_17"));
                Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("v11_table_test_table_15"));
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateReferentialAction_ReturnsUpdateReferentialActionAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_21", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_22", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_23", System.StringComparison.Ordinal));

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteReferentialAction_ReturnsDeleteReferentialActionAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_27", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_28", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_29", System.StringComparison.Ordinal));

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetDefault));
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "v11_table_test_table_17", System.StringComparison.Ordinal));

            Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
        }
    }
}