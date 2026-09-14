using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSqlTest
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
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeUpdateReferentialAction_ReturnsUpdateReferentialActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_18", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_19", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetDefault()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_20", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_16", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithCascadeDeleteReferentialAction_ReturnsDeleteReferentialActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_24", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetNullDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_25", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToPrimaryKeyWithSetDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetDefault()
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
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeUpdateReferentialAction_ReturnsUpdateReferentialActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_21", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_22", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultUpdateReferentialAction_ReturnsUpdateReferentialActionAsSetDefault()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_23", StringComparison.Ordinal));

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.SetDefault));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithCascadeDeleteReferentialAction_ReturnsDeleteReferentialActionAsCascade()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_27", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.Cascade));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetNullDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetNull()
    {
        var table = await GetTableAsync("table_test_table_15");
        var childKeys = table.ChildKeys;
        var foreignKey = childKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_28", StringComparison.Ordinal));

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToUniqueKeyWithSetDefaultDeleteReferentialAction_ReturnsDeleteReferentialActionAsSetDefault()
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
    public async Task ChildKeys_WhenGivenChildTableWithForeignKey_ReturnsValidatedNotDeferrableAndSimpleMatch()
    {
        var table = await GetTableAsync("table_test_table_15");
        var foreignKey = table.ChildKeys.Single(k => string.Equals(k.ChildTable.LocalName, "table_test_table_17", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.IsValidated, Is.True);
            Assert.That(foreignKey.ChildKey.Deferrability, Is.EqualTo(ConstraintDeferrability.NotDeferrable));
            Assert.That(foreignKey.MatchType, Is.EqualTo(ForeignKeyMatchType.Simple));
            Assert.That(foreignKey.SetNullColumns, Is.Empty);
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenNotValidDeferrableMatchFullForeignKey_ReturnsDeclaredConstraintState()
    {
        var table = await GetTableAsync("constraint_state_fk_parent");
        var foreignKey = table.ChildKeys.Single();

        var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
        var expectedChildColumns = new[] { "a", "b" };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("constraint_state_child"));
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_constraint_state_child"));
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(foreignKey.ChildKey.IsValidated, Is.False);
            Assert.That(foreignKey.ChildKey.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyImmediate));
            Assert.That(foreignKey.MatchType, Is.EqualTo(ForeignKeyMatchType.Full));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenForeignKeyWithSetNullColumnSubset_ReturnsOnlyThoseColumns()
    {
        var version = await DatabaseProvider.GetDatabaseVersionAsync(TestContext.CurrentContext.CancellationToken);
        if (version.Major < 15)
            Assert.Ignore("ON DELETE SET NULL with a column subset requires PostgreSQL 15 or later.");

        var table = await GetTableAsync("fk_set_null_subset_parent");
        var foreignKey = table.ChildKeys.Single();

        var childColumns = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName);
        var setNullColumns = foreignKey.SetNullColumns.Select(c => c.Name.LocalName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.SetNull));
            Assert.That(childColumns, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(setNullColumns, Is.EqualTo(new[] { "b" }));
        }
    }

    [Test]
    public async Task ChildKeys_WhenGivenChildTableWithForeignKeyToBareUniqueIndex_ContainsConstraintWithIndexAsParentKey()
    {
        var table = await GetTableAsync("fk_bare_unique_parent");
        var foreignKey = table.ChildKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("fk_bare_unique_child"));
            Assert.That(foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "a" }));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_fk_bare_unique_parent"));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
        }
    }

    [Test]
    public async Task ChildKeys_WhenChildTableReferencesAnotherTable_OnlyContainsKeysReferencingThisTable()
    {
        var table = await GetTableAsync("child_key_round_trip_parent");
        var foreignKey = table.ChildKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("child_key_round_trip_child"));
            Assert.That(foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "parent_id" }));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("child_key_round_trip_parent"));
        }
    }
}