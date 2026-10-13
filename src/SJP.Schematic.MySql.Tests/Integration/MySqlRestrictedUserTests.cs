using System;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

/// <summary>
/// Loads views and routines as a user that can see them but not read their definitions: <c>SELECT</c> on a view
/// without <c>SHOW VIEW</c>, and <c>EXECUTE</c> on routines it did not define.
/// </summary>
internal sealed class MySqlRestrictedUserTests : MySqlTest
{
    private const string UserName = "schematic_restricted";
    private const string Password = "Schematic_Restricted1";

    private MySqlConnectionFactory _restrictedConnectionFactory = null!;

    private ISchematicConnection RestrictedConnection => new SchematicConnection(_restrictedConnectionFactory, new MySqlDialect());

    private IDatabaseViewProvider ViewProvider => new MySqlDatabaseViewProvider(RestrictedConnection, IdentifierDefaults);

    private IDatabaseRoutineProvider RoutineProvider => new MySqlDatabaseRoutineProvider(RestrictedConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await ExecuteBatchAsync(
            "create view restricted_user_view as select 1 as test_column",
            "create function restricted_user_function() returns int deterministic return 1",
            "create procedure restricted_user_procedure(in first_arg int, out second_arg varchar(50)) deterministic set second_arg = 'test'",
            $"create user '{UserName}'@'%' identified by '{Password}'",
            $"grant select on restricted_user_view to '{UserName}'@'%'",
            $"grant execute on function restricted_user_function to '{UserName}'@'%'",
            $"grant execute on procedure restricted_user_procedure to '{UserName}'@'%'"
        );

        var builder = new MySqlConnectionStringBuilder(Config.ConnectionString)
        {
            UserID = UserName,
            Password = Password,
        };
        _restrictedConnectionFactory = new MySqlConnectionFactory(builder.ConnectionString);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_restrictedConnectionFactory != null)
            await _restrictedConnectionFactory.DisposeAsync();

        await ExecuteBatchAsync(
            $"drop user if exists '{UserName}'@'%'",
            "drop view if exists restricted_user_view",
            "drop function if exists restricted_user_function",
            "drop procedure if exists restricted_user_procedure"
        );
    }

    [Test]
    public async Task GetView_WhenDefinitionUnreadable_ReturnsViewWithCommentDefinitionAndColumns()
    {
        var view = await ViewProvider.GetView("restricted_user_view").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(view.Definition, Does.StartWith("--"));
            Assert.That(view.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "test_column" }));
        }
    }

    [Test]
    public async Task GetAllViews_WhenDefinitionUnreadable_ReturnsViewWithCommentDefinition()
    {
        var views = await ViewProvider.GetAllViews();
        var view = views.Single(static v => v.Name.LocalName == "restricted_user_view");

        Assert.That(view.Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task EnumerateAllViews_WhenDefinitionUnreadable_ReturnsViewWithCommentDefinition()
    {
        var views = await ViewProvider.EnumerateAllViews().ToListAsync();
        var view = views.Single(static v => v.Name.LocalName == "restricted_user_view");

        Assert.That(view.Definition, Does.StartWith("--"));
    }

    [Test]
    public async Task GetRoutine_WhenFunctionDefinitionUnreadable_ReturnsFunctionWithCommentDefinitionAndReturnType()
    {
        var routine = await RoutineProvider.GetRoutine("restricted_user_function").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.Definition, Does.StartWith("--"));
            Assert.That(routine.RoutineType, Is.EqualTo(RoutineType.Function));
            Assert.That(routine.ReturnType, OptionIs.Some);
        }
    }

    [Test]
    public async Task GetRoutine_WhenProcedureDefinitionUnreadable_ReturnsProcedureWithCommentDefinitionAndParameters()
    {
        var routine = await RoutineProvider.GetRoutine("restricted_user_procedure").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.Definition, Does.StartWith("--"));
            Assert.That(routine.RoutineType, Is.EqualTo(RoutineType.Procedure));
            Assert.That(routine.Parameters, Has.Count.EqualTo(2));
        }
    }

    [Test]
    public async Task GetAllRoutines_WhenDefinitionsUnreadable_ReturnsRoutinesWithCommentDefinitions()
    {
        var routines = await RoutineProvider.GetAllRoutines();
        var definitions = routines
            .Where(static r => r.Name.LocalName.StartsWith("restricted_user_", StringComparison.Ordinal))
            .Select(static r => r.Definition)
            .ToList();

        Assert.That(definitions, Has.Count.EqualTo(2).And.All.StartWith("--"));
    }

    [Test]
    public async Task EnumerateAllRoutines_WhenDefinitionsUnreadable_ReturnsRoutinesWithCommentDefinitions()
    {
        var routines = await RoutineProvider.EnumerateAllRoutines().ToListAsync();
        var definitions = routines
            .Where(static r => r.Name.LocalName.StartsWith("restricted_user_", StringComparison.Ordinal))
            .Select(static r => r.Definition)
            .ToList();

        Assert.That(definitions, Has.Count.EqualTo(2).And.All.StartWith("--"));
    }
}
