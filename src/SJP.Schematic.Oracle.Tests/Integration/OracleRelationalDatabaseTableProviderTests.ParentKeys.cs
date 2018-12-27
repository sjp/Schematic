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
        public async Task ParentKeys_WhenGivenTableWithNoForeignKeys_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
            var count = table.ParentKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_16", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("PK_TEST_TABLE_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
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
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await GetTableAsync("table_test_table_24").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_25").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
        {
            var table = await GetTableAsync("table_test_table_30").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("FK_TEST_TABLE_17", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("UK_TEST_TABLE_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var foreignKey = table.ParentKeys.Single();

            Assert.Multiple(() =>
            {
                Assert.AreEqual("TABLE_TEST_TABLE_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("TABLE_TEST_TABLE_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
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
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await GetTableAsync("table_test_table_27").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await GetTableAsync("table_test_table_28").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ParentKeys_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
        {
            var table = await GetTableAsync("table_test_table_31").ConfigureAwait(false);
            var parentKeys = table.ParentKeys;
            var foreignKey = parentKeys.Single();

            Assert.IsFalse(foreignKey.ChildKey.IsEnabled);
        }
    }
}