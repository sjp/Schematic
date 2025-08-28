using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed partial class SqlServerRelationalDatabaseTableProviderTests : SqlServerTest
{
    [Test]
    public async Task UniqueKeys_WhenGivenTableWithNoUniqueKeys_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);

        Assert.That(table.UniqueKeys, Is.Empty);
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithSingleUniqueKey_ReturnsCorrectKeyType()
    {
        var table = await GetTableAsync("table_test_table_5").ConfigureAwait(false);
        var uk = table.UniqueKeys.Single();

        Assert.That(uk.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithColumnAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
    {
        var table = await GetTableAsync("table_test_table_5").ConfigureAwait(false);
        var uk = table.UniqueKeys.Single();
        var ukColumns = uk.Columns.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ukColumns, Has.Exactly(1).Items);
            Assert.That(ukColumns.Single().Name.LocalName, Is.EqualTo("test_column"));
        }
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnOnly()
    {
        var table = await GetTableAsync("table_test_table_6").ConfigureAwait(false);
        var uk = table.UniqueKeys.Single();
        var ukColumns = uk.Columns.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ukColumns, Has.Exactly(1).Items);
            Assert.That(ukColumns.Single().Name.LocalName, Is.EqualTo("test_column"));
        }
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithSingleColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_6").ConfigureAwait(false);
        var uk = table.UniqueKeys.Single();

        Assert.That(uk.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_6"));
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

        var table = await GetTableAsync("table_test_table_7").ConfigureAwait(false);
        var uk = table.UniqueKeys.Single();
        var ukColumns = uk.Columns.ToList();
        var ukColumnNames = ukColumns.ConvertAll(c => c.Name.LocalName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ukColumns, Has.Exactly(3).Items);
            Assert.That(ukColumnNames, Is.EqualTo(expectedColumnNames));
        }
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithMultiColumnConstraintAsUniqueKey_ReturnsUniqueKeyWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_7").ConfigureAwait(false);
        var uk = table.UniqueKeys.Single();

        Assert.That(uk.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test_table_7"));
    }
}