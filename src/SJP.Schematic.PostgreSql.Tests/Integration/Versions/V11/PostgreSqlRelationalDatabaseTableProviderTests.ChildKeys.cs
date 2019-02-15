using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql11Test
    {
        [Test]
        public async Task ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("v11_table_test_table_2").ConfigureAwait(false);
            var count = table.ChildKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("pk_test_table_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("v11_table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("v11_table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_18");

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_19");

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_20");

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_24");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_25");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_26");

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.UnwrapSome().LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.UnwrapSome().LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("v11_table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("v11_table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

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
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateRule_ReturnsUpdateRuleAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_21");

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_22");

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateRule_ReturnsUpdateRuleAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_23");

            Assert.AreEqual(Rule.SetDefault, foreignKey.UpdateRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_27");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_28");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteRule_ReturnsDeleteRuleAsSetDefault()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_29");

            Assert.AreEqual(Rule.SetDefault, foreignKey.DeleteRule);
        }

        [Test]
        public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v11_table_test_table_15").ConfigureAwait(false);
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "v11_table_test_table_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }
    }
}