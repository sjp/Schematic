using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
{
    [Test]
    public async Task Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);

        Assert.That(table.Checks, Is.Empty);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
        var check = table.Checks.Single();

        Assert.That(check.Name.UnwrapSome().LocalName, Is.EqualTo("ck_test_table_14"));
    }

    [Test]
    public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
    {
        const string expectedDefinition = "([test_column]>(1))";

        var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
        var check = table.Checks.Single();

        var comparer = new SqliteExpressionComparer();
        var checksEqual = comparer.Equals(expectedDefinition, check.Definition);

        Assert.That(checksEqual, Is.True);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
        var check = table.Checks.Single();

        Assert.That(check.IsEnabled, Is.True);
    }
}