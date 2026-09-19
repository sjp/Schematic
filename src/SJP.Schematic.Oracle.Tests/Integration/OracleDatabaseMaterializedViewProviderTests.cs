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

internal sealed class OracleDatabaseMaterializedViewProviderTests : OracleTest
{
    private IDatabaseViewProvider ViewProvider => new OracleDatabaseMaterializedViewProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view mview_db_test_view_1 as select 1 as dummy from dual", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create view mview_view_test_view_1 as select 1 as test from dual", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table mview_view_test_table_1 (table_id number)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create materialized view mview_view_test_view_2 as select table_id as test from mview_view_test_table_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table mview_view_test_table_2 (not_null_column number not null, nullable_column number)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create materialized view mview_view_test_view_3 as select not_null_column, nullable_column from mview_view_test_table_2", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table mview_view_test_table_3 (test_column varchar2(20))", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create materialized view mview_view_test_view_4 as select test_column from mview_view_test_table_3", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create index mview_view_test_view_4_ix_1 on mview_view_test_view_4 (upper(test_column))", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table mview_view_test_table_4 (test_column number)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create materialized view mview_view_test_view_5 as select test_column from mview_view_test_table_4", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger mview_view_test_view_5_trigger_1
before insert on mview_view_test_view_5
for each row
begin
    null;
end;
", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop view mview_db_test_view_1",
        "drop view mview_view_test_view_1",
        "drop materialized view mview_view_test_view_2",
        "drop table mview_view_test_table_1",
        "drop materialized view mview_view_test_view_3",
        "drop table mview_view_test_table_2",
        "drop materialized view mview_view_test_view_4",
        "drop table mview_view_test_table_3",
        "drop materialized view mview_view_test_view_5",
        "drop table mview_view_test_table_4");

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
        var viewIsSome = await ViewProvider.GetView("mview_view_test_view_2").IsSome;
        Assert.That(viewIsSome, Is.True);
    }

    [Test]
    public async Task GetView_WhenViewPresent_ReturnsViewWithCorrectName()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "mview_view_test_view_2");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");
        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("mview_view_test_view_2");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "mview_view_test_view_2");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "mview_view_test_view_2");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "mview_view_test_view_2");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "mview_view_test_view_2");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "MVIEW_VIEW_TEST_VIEW_2");

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
    public async Task GetView_WhenGivenQueryViewName_ReturnsNone()
    {
        var viewIsNone = await ViewProvider.GetView("mview_view_test_view_1").IsNone;
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
        const string viewName = "MVIEW_VIEW_TEST_VIEW_2";
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
        const string viewName = "MVIEW_VIEW_TEST_VIEW_2";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_DoesNotContainQueryView()
    {
        const string viewName = "MVIEW_VIEW_TEST_VIEW_1";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task GetAllViews_WhenRetrieved_DoesNotContainQueryView()
    {
        const string viewName = "MVIEW_VIEW_TEST_VIEW_1";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var view = await GetViewAsync("mview_view_test_view_2");

        var definition = view.Definition;
        const string expected = "select table_id as test from mview_view_test_table_1";

        Assert.That(definition, Is.EqualTo(expected));
    }

    [Test]
    public async Task IsMaterialized_WhenViewIsMaterialized_ReturnsTrue()
    {
        var view = await GetViewAsync("mview_view_test_view_2");

        Assert.That(view.IsMaterialized, Is.True);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsOneValueOnly()
    {
        var view = await GetViewAsync("mview_view_test_view_2");

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        const string expectedColumnName = "TEST";
        var view = await GetViewAsync("mview_view_test_view_2");
        var containsColumn = view.Columns.Any(c => c.Name == expectedColumnName);

        Assert.That(containsColumn, Is.True);
    }

    [Test]
    public async Task Columns_WhenSelectingNotNullTableColumn_ReturnsNotNullableColumn()
    {
        var view = await GetViewAsync("mview_view_test_view_3");
        var column = view.Columns.Single(c => c.Name.LocalName == "NOT_NULL_COLUMN");

        Assert.That(column.IsNullable, Is.False);
    }

    [Test]
    public async Task Columns_WhenSelectingNullableTableColumn_ReturnsNullableColumn()
    {
        var view = await GetViewAsync("mview_view_test_view_3");
        var column = view.Columns.Single(c => c.Name.LocalName == "NULLABLE_COLUMN");

        Assert.That(column.IsNullable, Is.True);
    }

    // a function-based index adds a system-generated hidden column to the view's container table
    [Test]
    public async Task Columns_WhenViewHasFunctionBasedIndex_ContainsOnlySelectedColumns()
    {
        var view = await GetViewAsync("mview_view_test_view_4");
        var columnNames = view.Columns.Select(c => c.Name.LocalName).ToList();

        Assert.That(columnNames, Is.EqualTo(new[] { "TEST_COLUMN" }));
    }

    [Test]
    public async Task Columns_WhenViewHasFunctionBasedIndex_ReturnsVisibleColumns()
    {
        var view = await GetViewAsync("mview_view_test_view_4");

        Assert.That(view.Columns.Select(c => c.IsHidden), Is.All.False);
    }

    [Test]
    public async Task Triggers_GivenViewWithNoTriggers_ReturnsEmptyCollection()
    {
        var view = await GetViewAsync("mview_view_test_view_2");

        Assert.That(view.Triggers, Is.Empty);
    }

    // a trigger created on a materialized view is attached to its container table
    [Test]
    public async Task Triggers_GivenViewWithTrigger_ReturnsTrigger()
    {
        const string triggerName = "MVIEW_VIEW_TEST_VIEW_5_TRIGGER_1";

        var view = await GetViewAsync("mview_view_test_view_5");
        var triggerNames = view.Triggers.Select(t => t.Name.LocalName).ToList();

        Assert.That(triggerNames, Is.EqualTo(new[] { triggerName }));
    }

    [Test]
    public async Task Triggers_GivenViewWithTriggerForInsert_ReturnsCorrectEventAndTiming()
    {
        var view = await GetViewAsync("mview_view_test_view_5");
        var trigger = view.Triggers.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(TriggerQueryTiming.Before));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(TriggerEvent.Insert));
        }
    }

    // A materialized view load issues 5 queries: one to resolve the view's name, then columns (including
    // their nullability), triggers, indexes, and the definition read together with the refresh options.
    [Test]
    public async Task GetView_WhenViewPresent_IssuesExpectedNumberOfRoundTrips()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var viewProvider = new OracleDatabaseMaterializedViewProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        _ = await viewProvider.GetView("mview_view_test_view_3", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(5));
    }
}