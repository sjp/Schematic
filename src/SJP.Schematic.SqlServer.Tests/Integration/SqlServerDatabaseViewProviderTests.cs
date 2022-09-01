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

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed class SqlServerDatabaseViewProviderTests : SqlServerTest
{
    private IDatabaseViewProvider ViewProvider => new SqlServerDatabaseViewProvider(Connection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create view view_test_view_1 as select 1 as test", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view view_test_view_2 with schemabinding as select table_id as test from [dbo].[view_test_table_1]", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create unique clustered index ix_view_test_view_2 on view_test_view_2 (test)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view db_test_view_1", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("drop view view_test_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view view_test_view_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table view_test_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IDatabaseView> GetViewAsync(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return GetViewAsyncCore(viewName);
    }

    private async Task<IDatabaseView> GetViewAsyncCore(Identifier viewName)
    {
        using (await _lock.LockAsync().ConfigureAwait(false))
        {
            if (!_viewsCache.TryGetValue(viewName, out var lazyView))
            {
                lazyView = new AsyncLazy<IDatabaseView>(() => ViewProvider.GetView(viewName).UnwrapSomeAsync());
                _viewsCache[viewName] = lazyView;
            }

            return await lazyView.ConfigureAwait(false);
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseView>> _viewsCache = new();

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsView()
    {
        var viewIsSome = await ViewProvider.GetView("db_test_view_1").IsSome.ConfigureAwait(false);
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_view_1");

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
    public async Task GetView_WhenViewPresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("DB_TEST_view_1");
        var view = await ViewProvider.GetView(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Dbo", "DB_TEST_view_1");
        var view = await ViewProvider.GetView(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, view.Name.Schema)
            && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, view.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetAllViews_WhenEnumerated_ContainsViews()
    {
        var hasViews = await ViewProvider.GetAllViews()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasViews, Is.True);
    }

    [Test]
    public async Task GetAllViews_WhenEnumerated_ContainsTestView()
    {
        const string viewName = "db_test_view_1";
        var containsTestView = await ViewProvider.GetAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var view = await GetViewAsync("view_test_view_1").ConfigureAwait(false);

        var definition = view.Definition;
        const string expected = "create view view_test_view_1 as select 1 as test";

        Assert.That(definition, Is.EqualTo(expected));
    }

    [Test]
    public async Task IsMaterialized_WhenViewIsNotIndexed_ReturnsFalse()
    {
        var view = await GetViewAsync("view_test_view_1").ConfigureAwait(false);

        Assert.That(view.IsMaterialized, Is.False);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
    {
        var view = await GetViewAsync("view_test_view_1").ConfigureAwait(false);

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        var view = await GetViewAsync("view_test_view_1").ConfigureAwait(false);
        var containsColumn = view.Columns.Any(c => c.Name == "test");

        Assert.That(containsColumn, Is.True);
    }

    [Test]
    public async Task IsMateralized_WhenViewHasSingleIndex_ReturnsTrue()
    {
        var view = await GetViewAsync("view_test_view_2").ConfigureAwait(false);

        Assert.That(view.IsMaterialized, Is.True);
    }
}