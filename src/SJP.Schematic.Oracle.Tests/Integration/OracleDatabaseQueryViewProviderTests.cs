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
        await DbConnection.ExecuteAsync(@"create table query_view_test_table_3 (
    timestamp_column timestamp,
    timestamp_tz_column timestamp(3) with time zone,
    interval_column interval day(0) to second(6),
    negative_scale_column number(5, -2),
    unbounded_precision_column number(*, 2)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_5 as select * from query_view_test_table_3", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"create table query_view_test_table_4 (
    number_column number,
    integer_column integer,
    nvarchar_column nvarchar2(100),
    nchar_column nchar(3),
    char_semantics_varchar_column varchar2(10 char),
    char_semantics_char_column char(5 char),
    byte_semantics_varchar_column varchar2(100 byte)
)", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_7 as select * from query_view_test_table_4", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync("create view query_view_test_view_6 as select not_null_column, nullable_column from query_view_test_table_2", TestContext.CurrentContext.CancellationToken);
        await DbConnection.ExecuteAsync(@"
create trigger query_view_test_view_6_trigger_1
instead of insert on query_view_test_view_6
for each row
begin
    null;
end;
", TestContext.CurrentContext.CancellationToken);
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop view query_db_test_view_1",
        "drop view query_view_test_view_1",
        "drop materialized view query_view_test_view_2",
        "drop table query_view_test_table_1",
        "drop view query_view_test_view_3",
        "drop view query_view_test_view_4",
        "drop view query_view_test_view_6",
        "drop table query_view_test_table_2",
        "drop view query_view_test_view_5",
        "drop table query_view_test_table_3",
        "drop view query_view_test_view_7",
        "drop table query_view_test_table_4");

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

    [Test]
    public async Task GetAllViews_WhenViewHasFractionalSecondsAndUnusualScaleColumns_ContainsView()
    {
        var views = await ViewProvider.GetAllViews();
        var containsView = views.Any(v => v.Name.LocalName == "QUERY_VIEW_TEST_VIEW_5");

        Assert.That(containsView, Is.True);
    }

    [Test]
    public async Task Columns_WhenViewHasFractionalSecondsAndUnusualScaleColumns_ReturnsExpectedPrecisions()
    {
        var view = await GetViewAsync("QUERY_VIEW_TEST_VIEW_5");
        var columns = view.Columns.ToDictionary(c => c.Name.LocalName, StringComparer.Ordinal);
        var negativeScale = columns["NEGATIVE_SCALE_COLUMN"].Type.NumericPrecision.UnwrapSome();
        var unboundedPrecision = columns["UNBOUNDED_PRECISION_COLUMN"].Type.NumericPrecision.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(columns["TIMESTAMP_COLUMN"].Type.FractionalSecondsPrecision.UnwrapSome(), Is.EqualTo(6));
            Assert.That(columns["TIMESTAMP_TZ_COLUMN"].Type.FractionalSecondsPrecision.UnwrapSome(), Is.EqualTo(3));
            Assert.That(columns["INTERVAL_COLUMN"].Type.FractionalSecondsPrecision.UnwrapSome(), Is.EqualTo(6));
            Assert.That(columns["TIMESTAMP_COLUMN"].Type.NumericPrecision, OptionIs.None);
            Assert.That(columns["INTERVAL_COLUMN"].Type.NumericPrecision, OptionIs.None);
            Assert.That(negativeScale.Precision, Is.EqualTo(5));
            Assert.That(negativeScale.Scale, Is.Zero);
            Assert.That(unboundedPrecision.Precision, Is.EqualTo(38));
            Assert.That(unboundedPrecision.Scale, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task Triggers_WhenViewHasNoTriggers_ReturnsEmptyCollection()
    {
        var view = await GetViewAsync("query_view_test_view_3");

        Assert.That(view.Triggers, Is.Empty);
    }

    [Test]
    public async Task Triggers_WhenViewHasInsteadOfTrigger_ReturnsTrigger()
    {
        var view = await GetViewAsync("query_view_test_view_6");
        var trigger = view.Triggers.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.Name.LocalName, Is.EqualTo("QUERY_VIEW_TEST_VIEW_6_TRIGGER_1"));
            Assert.That(trigger.QueryTiming, Is.EqualTo(TriggerQueryTiming.InsteadOf));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(TriggerEvent.Insert));
        }
    }

    // the catalog reports a NUMBER's storage size and the byte length of a length declared in
    // characters, neither of which is part of the column's declaration
    [TestCase("NUMBER_COLUMN", "\"NUMBER\"", 22, DataType.Numeric)]
    [TestCase("INTEGER_COLUMN", "\"NUMBER\"(38)", 22, DataType.BigInteger)]
    [TestCase("NVARCHAR_COLUMN", "\"NVARCHAR2\"(100)", 100, DataType.Unicode)]
    [TestCase("NCHAR_COLUMN", "\"NCHAR\"(3)", 3, DataType.Unicode)]
    [TestCase("CHAR_SEMANTICS_VARCHAR_COLUMN", "\"VARCHAR2\"(10 CHAR)", 10, DataType.String)]
    [TestCase("CHAR_SEMANTICS_CHAR_COLUMN", "\"CHAR\"(5 CHAR)", 5, DataType.String)]
    [TestCase("BYTE_SEMANTICS_VARCHAR_COLUMN", "\"VARCHAR2\"(100)", 100, DataType.String)]
    public async Task Columns_WhenGivenViewWithLengthOrPrecisionColumn_ReturnsDeclaredType(string columnName, string expectedDefinition, int expectedMaxLength, DataType expectedDataType)
    {
        var view = await GetViewAsync("query_view_test_view_7");
        var column = view.Columns.Single(c => c.Name.LocalName == columnName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.Type.Definition, Is.EqualTo(expectedDefinition));
            Assert.That(column.Type.MaxLength, Is.EqualTo(expectedMaxLength));
            Assert.That(column.Type.DataType, Is.EqualTo(expectedDataType));
        }
    }

    // A view load issues 3 queries: one to resolve the view's name, then columns (including their
    // nullability) alongside the definition, which is read together with the check option, the
    // updatability and whether the view has INSTEAD OF triggers. The trigger query only follows when
    // it does.
    [TestCase("query_view_test_view_3", 3)]
    [TestCase("query_view_test_view_6", 4)]
    public async Task GetView_WhenViewPresent_IssuesExpectedNumberOfRoundTrips(string viewName, int expectedQueryCount)
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var viewProvider = new OracleDatabaseQueryViewProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        _ = await viewProvider.GetView(viewName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(expectedQueryCount));
    }
}