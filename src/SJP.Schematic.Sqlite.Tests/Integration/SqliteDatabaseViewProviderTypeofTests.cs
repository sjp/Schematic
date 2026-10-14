using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// Views whose expression columns are typed from the runtime type of their first row, in a private
// in-memory database so that the views which fail to evaluate do not affect other fixtures.
internal sealed class SqliteDatabaseViewProviderTypeofTests : SqliteTest
{
    private const int WideViewColumnCount = 600;

    private CachingConnectionFactory _connectionFactory;

    [OneTimeSetUp]
    public async Task Init()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source=ViewTypeof_{Guid.NewGuid():N};Mode=Memory;Cache=Shared"));

        await _connectionFactory.ExecuteAsync("create table typeof_table (id integer primary key, name text, j text)", cancellationToken);
        await _connectionFactory.ExecuteAsync("insert into typeof_table values (1, null, 'not json'), (2, 'b', '[1]')", cancellationToken);
        await _connectionFactory.ExecuteAsync("create table typeof_empty_table (id integer primary key, name text)", cancellationToken);

        await _connectionFactory.ExecuteAsync("create view typeof_runtime_error as select id, json(j) as js from typeof_table order by id", cancellationToken);
        await _connectionFactory.ExecuteAsync("create view typeof_overflow as select abs(-9223372036854775807 - 1) as big", cancellationToken);
        await _connectionFactory.ExecuteAsync("create view typeof_null_first as select id, name || 'x' as label from typeof_table order by id", cancellationToken);
        await _connectionFactory.ExecuteAsync("create view typeof_empty as select id + 1 as next_id, name || 'x' as label from typeof_empty_table", cancellationToken);
        await _connectionFactory.ExecuteAsync("create view typeof_values as select id + 1 as next_id, 'a' || id as label, id / 2.0 as half, x'00' as bytes from typeof_table order by id", cancellationToken);

        var wideColumns = Enumerable.Range(1, WideViewColumnCount).Select(static n => $"id + {n} as c{n}").Join(", ");
        await _connectionFactory.ExecuteAsync($"create view typeof_wide as select {wideColumns} from typeof_table order by id", cancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();
    }

    private IDatabaseViewProvider CreateViewProvider()
    {
        var connection = new SchematicConnection(_connectionFactory, Dialect);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");
        return new SqliteDatabaseViewProvider(connection, new ConnectionPragma(connection), identifierDefaults);
    }

    private async Task<IDatabaseView> GetViewAsync(string viewName)
    {
        var view = await CreateViewProvider().GetView(viewName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        return view;
    }

    [Test]
    public async Task GetView_WhenFirstRowRaisesRuntimeError_ReturnsColumnsWithBlobAffinity()
    {
        var view = await GetViewAsync("typeof_runtime_error");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(view.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "id", "js" }));
            Assert.That(view.Columns.Select(static c => c.Type.DataType), Is.EqualTo(new[] { DataType.BigInteger, DataType.LargeBinary }));
        }
    }

    [Test]
    public async Task GetView_WhenEvaluatingViewOverflows_ReturnsColumnsWithBlobAffinity()
    {
        var view = await GetViewAsync("typeof_overflow");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(view.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "big" }));
            Assert.That(view.Columns.Select(static c => c.Type.DataType), Is.EqualTo(new[] { DataType.LargeBinary }));
        }
    }

    [Test]
    public async Task GetAllViews_WhenViewsRaiseRuntimeErrors_ReturnsEveryView()
    {
        var views = await CreateViewProvider().GetAllViews(TestContext.CurrentContext.CancellationToken);
        var expectedViewNames = new[]
        {
            "typeof_empty",
            "typeof_null_first",
            "typeof_overflow",
            "typeof_runtime_error",
            "typeof_values",
            "typeof_wide",
        };

        Assert.That(views.Select(static v => v.Name.LocalName).Order(StringComparer.Ordinal), Is.EqualTo(expectedViewNames));
    }

    [Test]
    public async Task GetView_WhenFirstValueIsNull_ReturnsColumnWithBlobAffinity()
    {
        var view = await GetViewAsync("typeof_null_first");

        Assert.That(view.Columns.Select(static c => c.Type.DataType), Is.EqualTo(new[] { DataType.BigInteger, DataType.LargeBinary }));
    }

    [Test]
    public async Task GetView_WhenViewIsEmpty_ReturnsExpressionColumnsWithBlobAffinity()
    {
        var view = await GetViewAsync("typeof_empty");

        Assert.That(view.Columns.Select(static c => c.Type.DataType), Is.EqualTo(new[] { DataType.LargeBinary, DataType.LargeBinary }));
    }

    [Test]
    public async Task GetView_WhenFirstRowHasValues_ReturnsColumnsTypedByRuntimeType()
    {
        var view = await GetViewAsync("typeof_values");

        Assert.That(view.Columns.Select(static c => c.Type.DataType), Is.EqualTo(new[] { DataType.BigInteger, DataType.UnicodeText, DataType.Float, DataType.LargeBinary }));
    }

    [Test]
    public async Task GetView_WhenViewHasMoreThan500ExpressionColumns_ReturnsColumnsTypedByRuntimeType()
    {
        var view = await GetViewAsync("typeof_wide");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(view.Columns, Has.Exactly(WideViewColumnCount).Items);
            Assert.That(view.Columns.Select(static c => c.Type.DataType), Is.All.EqualTo(DataType.BigInteger));
        }
    }

    [Test]
    public async Task GetView_WhenColumnsHaveSameRuntimeType_SharesColumnType()
    {
        var view = await GetViewAsync("typeof_wide");

        var firstType = view.Columns[0].Type;
        Assert.That(view.Columns.Select(static c => c.Type), Is.All.SameAs(firstType));
    }

    [Test]
    public async Task GetView_WhenViewsHaveColumnsWithSameDeclaredType_SharesColumnType()
    {
        var viewProvider = CreateViewProvider();
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var firstView = await viewProvider.GetView("typeof_runtime_error", cancellationToken).UnwrapSomeAsync();
        var secondView = await viewProvider.GetView("typeof_null_first", cancellationToken).UnwrapSomeAsync();

        Assert.That(secondView.Columns[0].Type, Is.SameAs(firstView.Columns[0].Type));
    }
}
