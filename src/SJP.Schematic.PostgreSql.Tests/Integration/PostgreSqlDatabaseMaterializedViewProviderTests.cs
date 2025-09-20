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

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseMaterializedViewProviderTests : PostgreSqlTest
{
    private IDatabaseViewProvider ViewProvider => new PostgreSqlDatabaseMaterializedViewProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view matview_db_test_view_1 as select 1 as dummy", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create view matview_view_test_view_1 as select 1 as test", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table matview_view_test_table_1 (table_id int primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view matview_view_test_view_2 as select table_id as test from matview_view_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create materialized view matview_view_test_matview_1 as select table_id as test from matview_view_test_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view matview_db_test_view_1", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("drop view matview_view_test_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view matview_view_test_view_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop materialized view matview_view_test_matview_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table matview_view_test_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IDatabaseView> GetViewAsync(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        lock (_lock)
        {
            if (!_viewsCache.TryGetValue(viewName, out var lazyView))
            {
                lazyView = new AsyncLazy<IDatabaseView>(() => ViewProvider.GetView(viewName).UnwrapSomeAsync());
                _viewsCache[viewName] = lazyView;
            }

            return lazyView.Task;
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseView>> _viewsCache = [];

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsView()
    {
        var viewIsSome = await ViewProvider.GetView("matview_view_test_matview_1").IsSome.ConfigureAwait(false);
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("matview_view_test_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "matview_view_test_matview_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewMissing_ReturnsNone()
    {
        var viewIsNone = await ViewProvider.GetView("view_that_doesnt_exist").IsNone.ConfigureAwait(false);
        Assert.That(viewIsNone, Is.True);
    }

    [Test]
    public async Task GetView_WhenGivenNameOfQueryView_ReturnsNone()
    {
        var viewIsNone = await ViewProvider.GetView("matview_db_test_view_1").IsNone.ConfigureAwait(false);
        Assert.That(viewIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_ContainsViews()
    {
        var hasViews = await ViewProvider.EnumerateAllViews()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasViews, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_ContainsTestView()
    {
        const string viewName = "matview_view_test_matview_1";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_DoesNotContainQueryView()
    {
        const string viewName = "matview_view_test_view_1";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task GetAllViews2_WhenRetrieved_ContainsViews()
    {
        var views = await ViewProvider.GetAllViews2().ConfigureAwait(false);

        Assert.That(views, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllViews2_WhenRetrieved_ContainsTestView()
    {
        const string viewName = "matview_view_test_matview_1";
        var views = await ViewProvider.GetAllViews2().ConfigureAwait(false);
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task GetAllViews2_WhenRetrieved_DoesNotContainQueryView()
    {
        const string viewName = "matview_view_test_view_1";
        var views = await ViewProvider.GetAllViews2().ConfigureAwait(false);
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        var definition = view.Definition;
        var version = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);

        var expected = version.Major >= 16
            ? @" SELECT table_id AS test
   FROM matview_view_test_table_1;"
            : @" SELECT matview_view_test_table_1.table_id AS test
   FROM matview_view_test_table_1;";

        Assert.That(definition, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "matview_view_test_matview_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);
        var containsColumn = view.Columns.Any(c => c.Name == "test");

        Assert.That(containsColumn, Is.True);
    }

    [Test]
    public async Task IsMaterialized_WhenViewIsMaterialized_ReturnsTrue()
    {
        var view = await GetViewAsync("matview_view_test_matview_1").ConfigureAwait(false);

        Assert.That(view.IsMaterialized, Is.True);
    }
}