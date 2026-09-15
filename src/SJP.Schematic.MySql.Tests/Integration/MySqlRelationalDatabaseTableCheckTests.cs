using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed class MySqlRelationalDatabaseTableCheckTests : MySqlTest
{
    private IRelationalDatabaseTableProvider TableProvider => new MySqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults);

    // MariaDB names unnamed check constraints per table (CONSTRAINT_1, CONSTRAINT_2, ...), so both of these
    // tables get a check with the same name there. MySQL names them after the table instead.
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_check_test_1 ( `first_value` int, check (`first_value` > 0) )", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table table_check_test_2 ( second_value int, check (second_value < 5) )", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public Task CleanUp() => DropTablesAsync(
        "table_check_test_1",
        "table_check_test_2"
    );

    [Test]
    public async Task Checks_WhenGivenTableWithUnnamedCheck_ReturnsOnlyThatTablesCheck()
    {
        var table = await TableProvider.GetTable("table_check_test_1").UnwrapSomeAsync();

        Assert.That(table.Checks, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Checks_WhenAnotherTableHasUnnamedCheck_DoesNotContainTheOtherTablesCheck()
    {
        var table = await TableProvider.GetTable("table_check_test_2").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Checks, Has.Exactly(1).Items);
            Assert.That(table.Checks.Single().Definition, Does.Contain("second_value"));
            Assert.That(table.Checks.Single().IsEnabled, Is.True);
        }
    }
}
