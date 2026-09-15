using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed class OracleDatabaseQueryViewProviderTests : OracleTest
{
    private IDatabaseViewProvider ViewProvider => new OracleDatabaseQueryViewProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view query_db_test_view_1 as select 1 as dummy from dual", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create view query_view_test_view_1 as select 1 as test from dual", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table query_view_test_table_1 (table_id number)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create materialized view query_view_test_view_2 as select table_id as test from query_view_test_table_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table query_view_test_table_2 (not_null_column number not null, nullable_column number)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_3 as select not_null_column, nullable_column from query_view_test_table_2", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_4 (visible_column, invisible_column invisible) as select not_null_column, nullable_column from query_view_test_table_2", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await ExecuteBatchAsync(
            "drop view query_db_test_view_1",
            "drop view query_view_test_view_1",
            "drop materialized view query_view_test_view_2",
            "drop table query_view_test_table_1",
            "drop view query_view_test_view_3",
            "drop view query_view_test_view_4",
            "drop table query_view_test_table_2");
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
        var viewIsSome = await ViewProvider.GetView("query_db_test_view_1").IsSome;
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");
        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "QUERY_DB_TEST_VIEW_1");

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
    public async Task GetView_WhenGivenMaterializedViewName_ReturnsNone()
    {
        var viewIsNone = await ViewProvider.GetView("query_view_test_view_2").IsNone;
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
        const string viewName = "QUERY_DB_TEST_VIEW_1";
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
        const string viewName = "QUERY_DB_TEST_VIEW_1";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_DoesNotContainMaterializedView()
    {
        const string viewName = "QUERY_VIEW_TEST_VIEW_2";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task GetAllViews_WhenRetrieved_DoesNotContainMaterializedView()
    {
        const string viewName = "QUERY_VIEW_TEST_VIEW_2";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var view = await GetViewAsync("query_view_test_view_1");

        var definition = view.Definition;
        const string expected = "select 1 as test from dual";

        Assert.That(definition, Is.EqualTo(expected));
    }

    [Test]
    public async Task IsMaterialized_WhenViewIsNotMaterialized_ReturnsFalse()
    {
        var view = await GetViewAsync("query_view_test_view_1");

        Assert.That(view.IsMaterialized, Is.False);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
    {
        var view = await GetViewAsync("query_view_test_view_1");

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        const string expectedColumnName = "TEST";
        var view = await GetViewAsync("query_view_test_view_1");
        var containsColumn = view.Columns.Any(c => c.Name == expectedColumnName);

        Assert.That(containsColumn, Is.True);
    }

    [Test]
    public async Task Columns_WhenSelectingNotNullTableColumn_ReturnsNotNullableColumn()
    {
        var view = await GetViewAsync("query_view_test_view_3");
        var column = view.Columns.Single(c => c.Name.LocalName == "NOT_NULL_COLUMN");

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public async Task Columns_WhenSelectingNullableTableColumn_ReturnsNullableColumn()
    {
        var view = await GetViewAsync("query_view_test_view_3");
        var column = view.Columns.Single(c => c.Name.LocalName == "NULLABLE_COLUMN");

        Assert.That(column.IsNullable, Is.True);
    }

    [Test]
    public async Task Columns_WhenColumnDeclaredInvisible_ReturnsHiddenColumn()
    {
        var view = await GetViewAsync("query_view_test_view_4");
        var column = view.Columns.Single(c => c.Name.LocalName == "INVISIBLE_COLUMN");

        Assert.That(column.IsHidden, Is.True);
    }

    [Test]
    public async Task Columns_WhenColumnNotDeclaredInvisible_ReturnsVisibleColumn()
    {
        var view = await GetViewAsync("query_view_test_view_4");
        var column = view.Columns.Single(c => c.Name.LocalName == "VISIBLE_COLUMN");

        Assert.That(column.IsHidden, Is.False);
    }

    [Test]
    public async Task Columns_WhenViewHasInvisibleColumn_ReturnsInvisibleColumnLast()
    {
        var view = await GetViewAsync("query_view_test_view_4");
        var columnNames = view.Columns.Select(c => c.Name.LocalName).ToList();

        Assert.That(columnNames, Is.EqualTo(new[] { "VISIBLE_COLUMN", "INVISIBLE_COLUMN" }));
    }

    // A view load issues 4 queries: one to resolve the view's name, then columns (including their
    // nullability), INSTEAD OF triggers, and the definition read together with the check option and
    // updatability.
    [Test]
    public async Task GetView_WhenViewPresent_IssuesExpectedNumberOfRoundTrips()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var viewProvider = new OracleDatabaseQueryViewProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        _ = await viewProvider.GetView("query_view_test_view_3", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(4));
    }
}