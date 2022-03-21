using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    [Test]
    public async Task ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_2").ConfigureAwait(false);

        Assert.That(table.ChildKeys, Is.Empty);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("FK_TEST_TABLE_16"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("PK_TEST_TABLE_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_16"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
        var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

        var expectedChildColumns = new[] { "FIRST_NAME_CHILD" };
        var expectedParentColumns = new[] { "FIRST_NAME_PARENT" };

        Assert.Multiple(() =>
        {
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_24", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_25", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_16", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_30", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.False);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("FK_TEST_TABLE_17"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("UK_TEST_TABLE_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_17"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
        var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

        var expectedChildColumns = new[] { "LAST_NAME_CHILD", "MIDDLE_NAME_CHILD" };
        var expectedParentColumns = new[] { "LAST_NAME_PARENT", "MIDDLE_NAME_PARENT" };

        Assert.Multiple(() =>
        {
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_27", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_28", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_17", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "TABLE_TEST_TABLE_31", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.False);
    }
}