using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
{
    [Test]
    public async Task ChildKeys_WhenGivenTableWithNoChildKeys_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_2");

        Assert.That(table.ChildKeys, Is.Empty);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectNames()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_16"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("pk_test_table_15"));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectKeyTypes()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectTables()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("table_test_table_16"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("table_test_table_15"));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ContainsConstraintWithCorrectColumns()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
        var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

        var expectedChildColumns = new[] { "first_name_child" };
        var expectedParentColumns = new[] { "first_name_parent" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_18", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_19", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateAction_ReturnsUpdateActionAsSetDefault()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_20", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_24", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_25", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteAction_ReturnsDeleteActionAsSetDefault()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_26", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetDefault));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKey_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test_table_17"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_15"));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectKeyTypes()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectTables()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("table_test_table_17"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("table_test_table_15"));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectColumns()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
        var parentColumns = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName);

        var expectedChildColumns = new[] { "last_name_child", "middle_name_child" };
        var expectedParentColumns = new[] { "last_name_parent", "middle_name_parent" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateAction_ReturnsUpdateActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_21", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateAction_ReturnsUpdateActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_22", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateAction_ReturnsUpdateActionAsSetDefault()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_23", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteAction_ReturnsDeleteActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_27", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteAction_ReturnsDeleteActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_28", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteAction_ReturnsDeleteActionAsSetDefault()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_29", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetDefault));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKey_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.True);
    }

    [Test]
    public async Task ChildKeys_WhenGivenTableWithChildKeysAndSelfReferencingKey_ContainsExpectedChildKeySet()
    {
        // this in particular is to fix a bug that will only happen when two conditions are met:
        // - a child table alphabetically appears before the parent table
        // - the parent table must also have a self-referencing foreign key

        var expectedChildTableNames = new[]
        {
            "table_test_table_34",
            "table_test_table_35",
            "table_test_table_36",
        };

        var table = await GetTableAsync("table_test_table_35");
        var childTableNames = table.ChildKeys.Select(ck => ck.ChildTable.LocalName).ToList();

        Assert.That(childTableNames, Is.EqualTo(expectedChildTableNames));
    }
}