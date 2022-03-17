﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
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
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("FK_TEST_TABLE_16"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("PK_TEST_TABLE_15"));
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
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_16"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_15"));
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

        Assert.Multiple(() =>
        {
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
        });
    }

    [Test]
    public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
        var parentKeys = table.ParentKeys;
        var foreignKey = parentKeys.Single();

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ParentKeys_WhenGivenTableWithForeignKeyToPrimaryKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_16").ConfigureAwait(false);
        var parentKeys = table.ParentKeys;
        var foreignKey = parentKeys.Single();

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
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
    public async Task ParentKeys_WhenGivenTableWithDisabledForeignKeyToPrimaryKey_ReturnsIsEnabledFalse()
    {
        var table = await GetTableAsync("table_test_table_30").ConfigureAwait(false);
        var parentKeys = table.ParentKeys;
        var foreignKey = parentKeys.Single();

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.False);
    }

    [Test]
    public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKey_ContainsConstraintWithCorrectNames()
    {
        var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
        var foreignKey = table.ParentKeys.Single();

        Assert.Multiple(() =>
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("FK_TEST_TABLE_17"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("UK_TEST_TABLE_15"));
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
            Assert.That(foreignKey.ChildTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_17"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("TABLE_TEST_TABLE_15"));
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

        Assert.Multiple(() =>
        {
            Assert.That(childColumns, Is.EqualTo(expectedChildColumns));
            Assert.That(parentColumns, Is.EqualTo(expectedParentColumns));
        });
    }

    [Test]
    public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultUpdateAction_ReturnsUpdateActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
        var parentKeys = table.ParentKeys;
        var foreignKey = parentKeys.Single();

        Assert.That(foreignKey.UpdateAction, Is.EqualTo(ReferentialAction.NoAction));
    }

    [Test]
    public async Task ParentKeys_WhenGivenTableWithForeignKeyToUniqueKeyWithDefaultDeleteAction_ReturnsDeleteActionAsNoAction()
    {
        var table = await GetTableAsync("table_test_table_17").ConfigureAwait(false);
        var parentKeys = table.ParentKeys;
        var foreignKey = parentKeys.Single();

        Assert.That(foreignKey.DeleteAction, Is.EqualTo(ReferentialAction.NoAction));
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

    [Test]
    public async Task ParentKeys_WhenGivenTableWithDisabledForeignKeyToUniqueKey_ReturnsIsEnabledFalse()
    {
        var table = await GetTableAsync("table_test_table_31").ConfigureAwait(false);
        var parentKeys = table.ParentKeys;
        var foreignKey = parentKeys.Single();

        Assert.That(foreignKey.ChildKey.IsEnabled, Is.False);
    }
}
