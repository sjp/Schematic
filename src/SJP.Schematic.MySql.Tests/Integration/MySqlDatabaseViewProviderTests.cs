using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed class MySqlDatabaseViewProviderTests : MySqlTest
{
    private IDatabaseViewProvider ViewProvider => new MySqlDatabaseViewProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy", CancellationToken.None);

        await DbConnection.ExecuteAsync("create view view_test_view_1 as select 1 as test", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)", CancellationToken.None);
        await DbConnection.ExecuteAsync("create view view_test_view_2 as select table_id as test from view_test_table_1", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view db_test_view_1", CancellationToken.None);

        await DbConnection.ExecuteAsync("drop view view_test_view_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop view view_test_view_2", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table view_test_table_1", CancellationToken.None);
    }

    private Task<IDatabaseView> GetViewAsync(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return GetViewAsyncCore(viewName);
    }

    private async Task<IDatabaseView> GetViewAsyncCore(Identifier viewName)
    {
        using (await _lock.LockAsync())
        {
            if (!_viewsCache.TryGetValue(viewName, out var lazyView))
            {
                lazyView = new AsyncLazy<IDatabaseView>(() => ViewProvider.GetView(viewName).UnwrapSomeAsync());
                _viewsCache[viewName] = lazyView;
            }

            return await lazyView;
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseView>> _viewsCache = [];

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsView()
    {
        var viewIsSome = await ViewProvider.GetView("db_test_view_1").IsSome;
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_GivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewMissing_ReturnsNone()
    {
        var viewIsNone = await ViewProvider.GetView("view_that_doesnt_exist").IsNone;
        Assert.That(viewIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_ContainsViews()
    {
        var hasViews = await ViewProvider.EnumerateAllViews().AnyAsync();

        Assert.That(hasViews, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_ContainsTestView()
    {
        const string viewName = "db_test_view_1";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task GetAllViews_WhenRetrieved_ContainsViews()
    {
        var views = await ViewProvider.GetAllViews();

        Assert.That(views, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllViews_WhenRetrieved_ContainsTestView()
    {
        const string viewName = "db_test_view_1";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var view = await GetViewAsync("view_test_view_1");

        var definition = view.Definition;
        const string expected = "select 1 AS `test`";

        Assert.That(definition, Is.EqualTo(expected));
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
        var view = await GetViewAsync(viewName);

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
        var view = await GetViewAsync(viewName);
        var containsColumn = view.Columns.Any(c => c.Name == "test");

        Assert.That(containsColumn, Is.True);
    }
}