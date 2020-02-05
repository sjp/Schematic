using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
    {
        [Test]
        public async Task ParentKeys_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);

            Assert.That(table.ParentKeys, Is.Empty);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_16"));
                Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("PRIMARY"));
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
                Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("table_test_table_16"));
                Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("table_test_table_15"));
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

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
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsRestrict()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Restrict));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
        {
            var table = await GetTableAsync("table_test_table_18").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_19").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsRestrict()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Restrict));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("table_test_table_24").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_25").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_17"));
                Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_15"));
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
                Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("table_test_table_17"));
                Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("table_test_table_15"));
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

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
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsRestrict()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Restrict));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
        {
            var table = await GetTableAsync("table_test_table_21").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_22").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsRestrict()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Restrict));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
        {
            var table = await GetTableAsync("table_test_table_27").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_28").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
        }
    }
}