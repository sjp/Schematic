using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseUserDefinedTypeProviderTests : PostgreSqlTest
{
    private IDatabaseUserDefinedTypeProvider TypeProvider => new PostgreSqlDatabaseUserDefinedTypeProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await ExecuteBatchAsync(
            "create domain db_test_udt_domain_1 as integer not null check (value > 0)",
            "create table db_test_udt_table_1 (test_column int)"
        );
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await ExecuteBatchAsync(
            "drop domain db_test_udt_domain_1",
            "drop table db_test_udt_table_1"
        );
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenLocalNameOnly_ReturnsTypeWithQualifiedName()
    {
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_domain_1");

        var type = await TypeProvider.GetUserDefinedType("db_test_udt_domain_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Name, Is.EqualTo(expectedTypeName));
            Assert.That(type.Kind, Is.EqualTo(UserDefinedTypeKind.Domain));
            Assert.That(type.Checks, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypeMissing_ReturnsNone()
    {
        var typeIsNone = await TypeProvider.GetUserDefinedType("db_test_udt_missing_1", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    // A table's row type lives in pg_type too, but it was not declared by a user.
    [Test]
    public async Task GetUserDefinedType_GivenTableRowTypeName_ReturnsNone()
    {
        var typeIsNone = await TypeProvider.GetUserDefinedType("db_test_udt_table_1", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    // The definition query also confirms the type exists, so a hit costs the definition query plus
    // the attribute and check queries, and a miss costs only the definition query.
    [Test]
    public async Task GetUserDefinedType_WhenTypePresent_IssuesNoSeparateNameQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var typeProvider = new PostgreSqlDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsSome = await typeProvider.GetUserDefinedType("db_test_udt_domain_1", TestContext.CurrentContext.CancellationToken).IsSome;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsSome, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(3));
        }
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypeMissing_IssuesOneQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var typeProvider = new PostgreSqlDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsNone = await typeProvider.GetUserDefinedType("db_test_udt_missing_1", TestContext.CurrentContext.CancellationToken).IsNone;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsNone, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }
}
