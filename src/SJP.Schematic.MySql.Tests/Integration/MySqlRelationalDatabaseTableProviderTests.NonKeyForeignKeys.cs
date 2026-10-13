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
    public async Task ParentKeys_WhenForeignKeyReferencesLeadingColumnOfWiderUniqueKey_IsNotReported()
    {
        var table = await GetTableAsync("non_key_fk_child_1");

        Assert.That(table.ParentKeys, Is.Empty);
    }

    [Test]
    public async Task ChildKeys_WhenForeignKeyReferencesLeadingColumnOfWiderUniqueKey_IsNotReported()
    {
        var table = await GetTableAsync("non_key_fk_parent_1");

        Assert.That(table.ChildKeys, Is.Empty);
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesNonUniqueIndex_IsNotReported()
    {
        var table = await GetTableAsync("non_key_fk_child_2");

        Assert.That(table.ParentKeys, Is.Empty);
    }

    [Test]
    public async Task ChildKeys_WhenForeignKeyReferencesNonUniqueIndex_IsNotReported()
    {
        var table = await GetTableAsync("non_key_fk_parent_2");

        Assert.That(table.ChildKeys, Is.Empty);
    }

    [Test]
    public async Task ParentKeys_WhenUniqueKeyHasExactlyTheReferencedColumns_ReferencesThatKey()
    {
        var table = await GetTableAsync("non_key_fk_child_3");
        var foreignKey = table.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_non_key_fk_child_3"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_non_key_fk_parent_3_d"));
            Assert.That(foreignKey.ParentKey.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "d" }));
        }
    }

    [Test]
    public async Task ChildKeys_WhenUniqueKeyHasExactlyTheReferencedColumns_ReferencesThatKey()
    {
        var table = await GetTableAsync("non_key_fk_parent_3");
        var foreignKey = table.ChildKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_non_key_fk_child_3"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_non_key_fk_parent_3_d"));
            Assert.That(foreignKey.ParentKey.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "d" }));
        }
    }

    [TestCase("non_key_fk_parent_1")]
    [TestCase("non_key_fk_child_1")]
    [TestCase("non_key_fk_parent_2")]
    [TestCase("non_key_fk_child_2")]
    [TestCase("non_key_fk_parent_3")]
    [TestCase("non_key_fk_child_3")]
    public async Task GetAllTables_WhenForeignKeysReferenceNonKeyIndexes_MatchesGetTable(string tableName)
    {
        var tables = await GetAllTables();
        var fromAll = tables.Single(t => string.Equals(t.Name.LocalName, tableName, StringComparison.Ordinal));
        var fromGet = await GetTableAsync(tableName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromAll.ParentKeys.Select(DescribeForeignKey), Is.EquivalentTo(fromGet.ParentKeys.Select(DescribeForeignKey)));
            Assert.That(fromAll.ChildKeys.Select(DescribeForeignKey), Is.EquivalentTo(fromGet.ChildKeys.Select(DescribeForeignKey)));
        }
    }

    private static string DescribeForeignKey(IDatabaseRelationalKey key)
    {
        var childColumns = string.Join(",", key.ChildKey.Columns.Select(static c => c.Name.LocalName));
        var parentColumns = string.Join(",", key.ParentKey.Columns.Select(static c => c.Name.LocalName));

        return $"{key.ChildTable.LocalName}.{key.ChildKey.Name.UnwrapSome().LocalName}({childColumns}) -> {key.ParentTable.LocalName}.{key.ParentKey.Name.UnwrapSome().LocalName}({parentColumns})";
    }
}
