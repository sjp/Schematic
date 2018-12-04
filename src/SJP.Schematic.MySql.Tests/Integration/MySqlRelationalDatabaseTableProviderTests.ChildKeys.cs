using System.Data;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
    {
        [Test]
        public void ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
        {
            var table = TableProvider.GetTable("table_test_table_2").UnwrapSome();
            var count = table.ChildKeys.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_16", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("PRIMARY", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Primary, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_16", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

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
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_18");

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_19");

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_24");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_25");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_16");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("fk_test_table_17", foreignKey.ChildKey.Name.LocalName);
                Assert.AreEqual("uk_test_table_15", foreignKey.ParentKey.Name.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(DatabaseKeyType.Foreign, foreignKey.ChildKey.KeyType);
                Assert.AreEqual(DatabaseKeyType.Unique, foreignKey.ParentKey.KeyType);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

            Assert.Multiple(() =>
            {
                Assert.AreEqual("table_test_table_17", foreignKey.ChildTable.LocalName);
                Assert.AreEqual("table_test_table_15", foreignKey.ParentTable.LocalName);
            });
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var foreignKey = table.ChildKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

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
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

            Assert.AreEqual(Rule.None, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateRule_ReturnsUpdateRuleAsCascade()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_21");

            Assert.AreEqual(Rule.Cascade, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateRule_ReturnsUpdateRuleAsSetNull()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_22");

            Assert.AreEqual(Rule.SetNull, foreignKey.UpdateRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteRule_ReturnsDeleteRuleAsNoAction()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

            Assert.AreEqual(Rule.None, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteRule_ReturnsDeleteRuleAsCascade()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_27");

            Assert.AreEqual(Rule.Cascade, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteRule_ReturnsDeleteRuleAsSetNull()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_28");

            Assert.AreEqual(Rule.SetNull, foreignKey.DeleteRule);
        }

        [Test]
        public void ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
        {
            var table = TableProvider.GetTable("table_test_table_15").UnwrapSome();
            var childKeys = table.ChildKeys;
            var foreignKey = childKeys.Single(k => k.ChildTable.LocalName == "table_test_table_17");

            Assert.IsTrue(foreignKey.ChildKey.IsEnabled);
        }
    }
}