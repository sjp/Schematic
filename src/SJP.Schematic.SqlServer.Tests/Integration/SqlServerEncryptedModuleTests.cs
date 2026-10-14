using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

/// <summary>
/// Views, routines and triggers created <c>WITH ENCRYPTION</c> have no readable definition, but are
/// still reported, with a SQL comment standing in for the definition.
/// </summary>
internal sealed class SqlServerEncryptedModuleTests : SqlServerTest
{
    private IDatabaseViewProvider ViewProvider => new SqlServerDatabaseViewProvider(Connection, IdentifierDefaults);

    private IDatabaseRoutineProvider RoutineProvider => new SqlServerDatabaseRoutineProvider(DbConnection, IdentifierDefaults);

    private IRelationalDatabaseTableProvider TableProvider => new SqlServerRelationalDatabaseTableProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view encrypted_test_view_1 with encryption as select 1 as test_column", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger encrypted_test_view_1_trigger_1
on encrypted_test_view_1
with encryption
instead of insert
as
begin
    declare @test int
end", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create procedure encrypted_test_procedure_1
    @test_parameter int
with encryption
as
select @test_parameter as test_column", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table encrypted_test_table_1 (table_id int primary key not null)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger encrypted_test_table_1_trigger_1
on encrypted_test_table_1
with encryption
after insert
as
begin
    declare @test int
end", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop view encrypted_test_view_1",
        "drop procedure encrypted_test_procedure_1",
        "drop table encrypted_test_table_1");

    [Test]
    public async Task GetView_WhenEncrypted_ReturnsViewWithCommentDefinitionAndColumns()
    {
        var view = await ViewProvider.GetView("encrypted_test_view_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(view.Definition, Does.StartWith("--"));
            Assert.That(view.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "test_column" }));
        }
    }

    [Test]
    public async Task GetView_WhenTriggerEncrypted_ReturnsTriggerWithCommentDefinition()
    {
        var view = await ViewProvider.GetView("encrypted_test_view_1").UnwrapSomeAsync();
        var trigger = view.Triggers.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.Name.LocalName, Is.EqualTo("encrypted_test_view_1_trigger_1"));
            Assert.That(trigger.Definition, Does.StartWith("--"));
            Assert.That(trigger.QueryTiming, Is.EqualTo(TriggerQueryTiming.InsteadOf));
        }
    }

    [Test]
    public async Task GetAllViews_WhenEncrypted_ReturnsViewWithCommentDefinition()
    {
        var views = await ViewProvider.GetAllViews();
        var view = views.Single(static v => v.Name.LocalName == "encrypted_test_view_1");

        Assert.That(view.Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task EnumerateAllViews_WhenEncrypted_ReturnsViewWithCommentDefinition()
    {
        var view = await ViewProvider.EnumerateAllViews()
            .SingleAsync(static v => v.Name.LocalName == "encrypted_test_view_1");

        Assert.That(view.Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task GetRoutine_WhenEncrypted_ReturnsRoutineWithCommentDefinitionAndParameters()
    {
        var routine = await RoutineProvider.GetRoutine("encrypted_test_procedure_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.Definition, Does.StartWith("--"));
            Assert.That(routine.RoutineType, Is.EqualTo(RoutineType.Procedure));
            Assert.That(routine.Parameters.Select(static p => p.Name.Match(static n => n.LocalName, static () => string.Empty)), Is.EqualTo(new[] { "@test_parameter" }));
        }
    }

    [Test]
    public async Task GetAllRoutines_WhenEncrypted_ReturnsRoutineWithCommentDefinition()
    {
        var routines = await RoutineProvider.GetAllRoutines();
        var routine = routines.Single(static r => r.Name.LocalName == "encrypted_test_procedure_1");

        Assert.That(routine.Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task EnumerateAllRoutines_WhenEncrypted_ReturnsRoutineWithCommentDefinition()
    {
        var routine = await RoutineProvider.EnumerateAllRoutines()
            .SingleAsync(static r => r.Name.LocalName == "encrypted_test_procedure_1");

        Assert.That(routine.Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task GetTable_WhenTriggerEncrypted_ReturnsTriggerWithCommentDefinition()
    {
        var table = await TableProvider.GetTable("encrypted_test_table_1").UnwrapSomeAsync();
        var trigger = table.Triggers.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.Name.LocalName, Is.EqualTo("encrypted_test_table_1_trigger_1"));
            Assert.That(trigger.Definition, Does.StartWith("--"));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(TriggerEvent.Insert));
        }
    }

    [Test]
    public async Task GetAllTables_WhenTriggerEncrypted_ReturnsTableWithCommentTriggerDefinition()
    {
        var tables = await TableProvider.GetAllTables();
        var table = tables.Single(static t => t.Name.LocalName == "encrypted_test_table_1");

        Assert.That(table.Triggers.Single().Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task EnumerateAllTables_WhenTriggerEncrypted_ReturnsTableWithCommentTriggerDefinition()
    {
        var table = await TableProvider.EnumerateAllTables()
            .SingleAsync(static t => t.Name.LocalName == "encrypted_test_table_1");

        Assert.That(table.Triggers.Single().Definition, Does.StartWith("--"));
    }
}
