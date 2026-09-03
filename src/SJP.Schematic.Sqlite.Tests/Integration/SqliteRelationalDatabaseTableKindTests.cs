using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed class SqliteRelationalDatabaseTableKindTests : SqliteTest
{
    public SqliteRelationalDatabaseTableKindTests()
    {
        TableProvider = new SqliteRelationalDatabaseTableProvider(Connection, Pragma, IdentifierDefaults);
    }

    private IRelationalDatabaseTableProvider TableProvider { get; }

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table kind_test_regular ( test_column integer )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table kind_test_without_rowid ( test_column integer not null primary key ) without rowid", CancellationToken.None);
        await DbConnection.ExecuteAsync("create virtual table kind_test_fts using fts5 ( test_column )", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table kind_test_regular", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table kind_test_without_rowid", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table kind_test_fts", CancellationToken.None);
    }

    [Test]
    public async Task GetTable_GivenOrdinaryTable_ReturnsRegularKind()
    {
        var table = await TableProvider.GetTable("kind_test_regular").UnwrapSomeAsync();

        Assert.That(table.Kind, Is.EqualTo(TableKind.Regular));
    }

    [Test]
    public async Task GetTable_GivenWithoutRowIdTable_ReturnsIndexOrganizedKind()
    {
        var table = await TableProvider.GetTable("kind_test_without_rowid").UnwrapSomeAsync();

        Assert.That(table.Kind, Is.EqualTo(TableKind.IndexOrganized));
    }

    [Test]
    public async Task GetTable_GivenVirtualTable_ReturnsVirtualKind()
    {
        var table = await TableProvider.GetTable("kind_test_fts").UnwrapSomeAsync();

        Assert.That(table.Kind, Is.EqualTo(TableKind.Virtual));
    }

    [Test]
    public async Task GetTable_GivenShadowTableOfVirtualTable_ReturnsNone()
    {
        // fts5 stores its index in tables of its own, e.g. kind_test_fts_data; those are an
        // implementation detail of the virtual table rather than tables in their own right
        var tableIsNone = await TableProvider.GetTable("kind_test_fts_data").IsNone;

        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public async Task GetAllTables_WhenInvoked_OmitsShadowTablesOfVirtualTables()
    {
        var tables = await TableProvider.GetAllTables();
        var tableNames = tables.Select(static t => t.Name.LocalName).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableNames, Has.Member("kind_test_fts"));
            Assert.That(tableNames, Has.No.Member("kind_test_fts_data"));
            Assert.That(tableNames, Has.No.Member("kind_test_fts_content"));
        }
    }

    [Test]
    public async Task GetTable_GivenOrdinaryTable_IsLogged()
    {
        var table = await TableProvider.GetTable("kind_test_regular").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.IsLogged, Is.True);
            Assert.That(table.Partitioning, OptionIs.None);
            Assert.That(table.SystemVersioning, OptionIs.None);
            Assert.That(table.Collation, OptionIs.None);
        }
    }
}
