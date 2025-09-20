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

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed class SqliteDatabaseViewProviderTests : SqliteTest
{
    private IDatabaseViewProvider ViewProvider => new SqliteDatabaseViewProvider(Connection, Pragma, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view db_test_view_1 as select 1 as dummy", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("create view view_test_view_1 as select 1 as test", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view view_test_view_2 as select 1, 2.345, 'test', X'DEADBEEF'", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view view_test_view_3 as select 1, 2.345, 'test', X'DEADBEEF', table_id from view_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view view_test_view_4 as select 1, 1, 1, 1", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view db_test_view_1", CancellationToken.None).ConfigureAwait(false);

        await DbConnection.ExecuteAsync("drop view view_test_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view view_test_view_3", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table view_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view view_test_view_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view view_test_view_4", CancellationToken.None).ConfigureAwait(false);
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
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseView>> _viewsCache = [];

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsView()
    {
        var viewIsSome = await ViewProvider.GetView("db_test_view_1").IsSome.ConfigureAwait(false);
        Assert.That(viewIsSome, Is.True);
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
        var containsTestView = await ViewProvider.GetAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, "db_test_view_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestView, Is.True);
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
        var views = await ViewProvider.GetAllViews2().ConfigureAwait(false);
        var containsTestView = views.Any(t => string.Equals(t.Name.LocalName, "db_test_view_1", StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalName_ShouldBeQualifiedCorrectly()
    {
        var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

        var view = await ViewProvider.GetView(expectedViewName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenOverlyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("test", IdentifierDefaults.Schema, "db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Schema, "db_test_view_1");

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
    public async Task GetView_WhenViewPresentGivenLocalNameNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("DB_TEST_view_1");
        var view = await ViewProvider.GetView(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenQualifiedNameNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Main", "DB_TEST_view_1");
        var view = await ViewProvider.GetView(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, view.Name);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        var definition = view.Definition;
        const string expected = "create view view_test_view_1 as select 1 as test";

        var definitionEqual = string.Equals(expected, definition, StringComparison.OrdinalIgnoreCase);
        Assert.That(definitionEqual, Is.True);
    }

    [Test]
    public async Task IsMaterialized_WhenViewIsNotMaterialized_ReturnsFalse()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        Assert.That(view.IsMaterialized, Is.False);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_1");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);
        var containsColumn = view.Columns.Any(c => c.Name == "test");

        Assert.That(containsColumn, Is.True);
    }

    [Test]
    public async Task Columns_WhenViewContainsUnnamedColumns_ContainsCorrectNumberOfColumns()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        Assert.That(view.Columns, Has.Exactly(4).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsUnnamedColumns_ContainsCorrectTypesForColumns()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_2");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);
        var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
        var expectedTypes = new[] { DataType.BigInteger, DataType.Float, DataType.UnicodeText, DataType.LargeBinary };

        Assert.That(columnTypes, Is.EqualTo(expectedTypes));
    }

    [Test]
    public async Task Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectNumberOfColumns()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        Assert.That(view.Columns, Has.Exactly(5).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsUnnamedColumnsAndTableColumn_ContainsCorrectTypesForColumns()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_3");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);
        var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
        var expectedTypes = new[] { DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.Numeric, DataType.BigInteger };

        Assert.That(columnTypes, Is.EqualTo(expectedTypes));
    }

    [Test]
    public async Task Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectNumberOfColumns()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);

        Assert.That(view.Columns, Has.Exactly(4).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsDuplicatedUnnamedColumns_ContainsCorrectTypesForColumns()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "view_test_view_4");
        var view = await GetViewAsync(viewName).ConfigureAwait(false);
        var columnTypes = view.Columns.Select(c => c.Type.DataType).ToList();
        var expectedTypes = new[] { DataType.BigInteger, DataType.BigInteger, DataType.BigInteger, DataType.BigInteger };

        Assert.That(columnTypes, Is.EqualTo(expectedTypes));
    }
}