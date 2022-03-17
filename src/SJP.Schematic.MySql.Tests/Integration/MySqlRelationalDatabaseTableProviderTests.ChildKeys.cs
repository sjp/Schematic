using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
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
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_16"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("PRIMARY"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

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
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("table_test_table_16"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("table_test_table_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

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
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsRestrictOrNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        // mysql treats restrict and no action as the same thing, accept either
        Assert.That(foreignKey.UpdateAction, Is.AnyOf(ReferentialAction.NoAction, ReferentialAction.Restrict));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_18", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_19", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsRestrictOrNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        // mysql treats restrict and no action as the same thing, accept either
        Assert.That(foreignKey.DeleteAction, Is.AnyOf(ReferentialAction.NoAction, ReferentialAction.Restrict));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_24", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_25", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_17"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

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
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("table_test_table_17"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("table_test_table_15"));
        });
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

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
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsRestrictOrNoAction()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        // mysql treats restrict and no action as the same thing, accept either
        Assert.That(foreignKey.UpdateAction, Is.AnyOf(ReferentialAction.NoAction, ReferentialAction.Restrict));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_21", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_22", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsRestrict()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        // mysql treats restrict and no action as the same thing, accept either
        Assert.That(foreignKey.DeleteAction, Is.AnyOf(ReferentialAction.NoAction, ReferentialAction.Restrict));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_27", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_28", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_15").ConfigureAwait(false);
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
    }
}
