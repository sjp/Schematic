using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseQueryViewProviderTests : PostgreSqlTest
{
    private IDatabaseViewProvider ViewProvider => new PostgreSqlDatabaseQueryViewProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view query_db_test_view_1 as select 1 as dummy", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create view query_view_test_view_1 as select 1 as test", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create table query_view_test_table_1 (table_id int primary key not null)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_2 as select table_id as test from query_view_test_table_1", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create materialized view query_view_test_matview_1 as select table_id as test from query_view_test_table_1", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create table query_view_test_table_2 (test_varchar varchar(50), test_numeric numeric(12, 4), test_float float8)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_3 as select test_varchar, test_numeric, test_float from query_view_test_table_2", TestContext.CurrentContext.CancellationToken);

        await DbConnection.ExecuteAsync("create view query_view_test_view_4 as select table_id as test from query_view_test_table_1 where table_id > 0 with local check option", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_5 as select table_id as test from query_view_test_table_1 where table_id > 0 with cascaded check option", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await ExecuteBatchAsync(
            "drop view query_db_test_view_1",
            "drop view query_view_test_view_1",
            "drop view query_view_test_view_2",
            "drop materialized view query_view_test_matview_1",
            "drop view query_view_test_view_3",
            "drop view query_view_test_view_4",
            "drop view query_view_test_view_5",
            "drop table query_view_test_table_2",
            "drop table query_view_test_table_1");
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
        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(viewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");

        var view = await ViewProvider.GetView(viewName).UnwrapSomeAsync();

        Assert.That(view.Name, Is.EqualTo(expectedViewName));
    }

    [Test]
    public async Task GetView_WhenViewPresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var viewName = new Identifier("A", "B", IdentifierDefaults.Schema, "query_db_test_view_1");
        var expectedViewName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "query_db_test_view_1");

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
    public async Task GetView_WhenGivenNameOfMaterializedView_ReturnsNone()
    {
        var viewIsNone = await ViewProvider.GetView("query_view_test_matview_1").IsNone;
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
        const string viewName = "query_db_test_view_1";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task EnumerateAllViews_WhenEnumerated_DoesNotContainMaterializedView()
    {
        const string viewName = "query_view_test_matview_1";
        var containsTestView = await ViewProvider.EnumerateAllViews()
            .AnyAsync(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
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
        const string viewName = "query_db_test_view_1";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.True);
    }

    [Test]
    public async Task GetAllViews_WhenRetrieved_DoesNotContainMaterializedView()
    {
        const string viewName = "query_view_test_matview_1";
        var views = await ViewProvider.GetAllViews();
        var containsTestView = views.Any(v => string.Equals(v.Name.LocalName, viewName, StringComparison.Ordinal));

        Assert.That(containsTestView, Is.False);
    }

    [Test]
    public async Task Definition_PropertyGet_ReturnsCorrectDefinition()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_view_test_view_1");
        var view = await GetViewAsync(viewName);

        var definition = view.Definition;
        const string expected = " SELECT 1 AS test;";

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
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_view_test_view_1");
        var view = await GetViewAsync(viewName);

        Assert.That(view.Columns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenViewContainsSingleColumn_ContainsColumnName()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_view_test_view_1");
        var view = await GetViewAsync(viewName);
        var containsColumn = view.Columns.Any(c => c.Name == "test");

        Assert.That(containsColumn, Is.True);
    }

    [Test]
    public async Task Columns_WhenViewColumnIsCharacterType_ContainsDeclaredMaxLength()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_view_test_view_3");
        var view = await GetViewAsync(viewName);
        var column = view.Columns.Single(c => c.Name == "test_varchar");

        Assert.That(column.Type.MaxLength, Is.EqualTo(50));
    }

    [Test]
    public async Task Columns_WhenViewColumnIsExactNumericType_ContainsDeclaredPrecisionAndScale()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_view_test_view_3");
        var view = await GetViewAsync(viewName);
        var column = view.Columns.Single(c => c.Name == "test_numeric");
        var precision = column.Type.NumericPrecision.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(precision.Precision, Is.EqualTo(12));
            Assert.That(precision.Scale, Is.EqualTo(4));
        }
    }

    // float8 declares 53 binary digits of precision, reported as the 16 decimal digits they span
    [Test]
    public async Task Columns_WhenViewColumnIsApproximateNumericType_ContainsPrecisionInDecimalDigits()
    {
        var viewName = new Identifier(IdentifierDefaults.Schema, "query_view_test_view_3");
        var view = await GetViewAsync(viewName);
        var column = view.Columns.Single(c => c.Name == "test_float");
        var precision = column.Type.NumericPrecision.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.Type.MaxLength, Is.EqualTo(16));
            Assert.That(precision.Precision, Is.EqualTo(16));
        }
    }
    [Test]
    public async Task CheckOption_WhenViewHasNoCheckOption_ReturnsNone()
    {
        var view = await GetViewAsync("query_view_test_view_2");

        Assert.That(view.CheckOption, Is.EqualTo(ViewCheckOption.None));
    }

    [Test]
    public async Task CheckOption_WhenViewHasLocalCheckOption_ReturnsLocal()
    {
        var view = await GetViewAsync("query_view_test_view_4");

        Assert.That(view.CheckOption, Is.EqualTo(ViewCheckOption.Local));
    }

    [Test]
    public async Task CheckOption_WhenViewHasCascadedCheckOption_ReturnsCascaded()
    {
        var view = await GetViewAsync("query_view_test_view_5");

        Assert.That(view.CheckOption, Is.EqualTo(ViewCheckOption.Cascaded));
    }

    [Test]
    public async Task IsUpdatable_WhenViewSelectsColumnOfSingleTable_ReturnsTrue()
    {
        var view = await GetViewAsync("query_view_test_view_2");

        Assert.That(view.IsUpdatable, Is.True);
    }

    [Test]
    public async Task IsUpdatable_WhenViewHasNoBaseTable_ReturnsFalse()
    {
        var view = await GetViewAsync("query_view_test_view_1");

        Assert.That(view.IsUpdatable, Is.False);
    }

    // A view load issues 4 queries: one to resolve the view's name, then its columns, its INSTEAD OF
    // triggers, and its definition read together with its check option and updatability.
    [Test]
    public async Task GetView_WhenViewPresent_IssuesExpectedNumberOfRoundTrips()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var viewProvider = new PostgreSqlDatabaseQueryViewProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        _ = await viewProvider.GetView("query_view_test_view_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(4));
    }
}