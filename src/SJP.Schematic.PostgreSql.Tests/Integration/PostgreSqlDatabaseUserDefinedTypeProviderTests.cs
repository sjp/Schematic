using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseUserDefinedTypeProviderTests : PostgreSqlTest
{
    private IDatabaseUserDefinedTypeProvider TypeProvider => new PostgreSqlDatabaseUserDefinedTypeProvider(DbConnection, IdentifierDefaults);

    private async Task<T> GetUserDefinedTypeAsync<T>(string typeName)
    {
        var type = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Value, Is.TypeOf<T>());
        return (T)type.Value!;
    }

    [OneTimeSetUp]
    public Task Init() => ExecuteBatchAsync(
        "create domain db_test_udt_domain_1 as integer not null check (value > 0)",
        "create domain db_test_udt_domain_2 as varchar(20) default 'abc'",
        "create type db_test_udt_enum_1 as enum ('small', 'medium', 'large')",
        "create type db_test_udt_composite_1 as (attr_one integer, attr_two text)",
        "create type db_test_udt_range_1 as range (subtype = float8)",
        "create table db_test_udt_table_1 (test_column int)"
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop domain db_test_udt_domain_1",
        "drop domain db_test_udt_domain_2",
        "drop type db_test_udt_enum_1",
        "drop type db_test_udt_composite_1",
        "drop type db_test_udt_range_1",
        "drop table db_test_udt_table_1"
    );

    [Test]
    public async Task GetUserDefinedType_ForDomainDeclaredNotNull_ReturnsNonNullableDomainWithAliasedTypeAndCheck()
    {
        var domain = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Domain>("db_test_udt_domain_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(domain.AliasedType.DataType, Is.EqualTo(DataType.Integer));
            Assert.That(domain.IsNullable, Is.False);
            Assert.That(domain.DefaultValue, OptionIs.None);
            Assert.That(domain.Checks.Single().Definition, Does.Contain("VALUE > 0"));
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForDomainWithDefault_ReturnsNullableDomainWithAliasedTypeAndDefault()
    {
        var domain = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Domain>("db_test_udt_domain_2");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(domain.AliasedType.MaxLength, Is.EqualTo(20));
            Assert.That(domain.IsNullable, Is.True);
            Assert.That(domain.DefaultValue.UnwrapSome(), Does.Contain("'abc'"));
            Assert.That(domain.Checks, Is.Empty);
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForEnum_ReturnsEnumWithValuesInDeclaredOrder()
    {
        var enumType = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Enum>("db_test_udt_enum_1");

        Assert.That(enumType.Values, Is.EqualTo(new[] { "small", "medium", "large" }));
    }

    [Test]
    public async Task GetUserDefinedType_ForCompositeType_ReturnsCompositeWithAttributesInDeclaredOrder()
    {
        var composite = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("db_test_udt_composite_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(composite.Attributes.Select(static a => a.Name.LocalName), Is.EqualTo(new[] { "attr_one", "attr_two" }));
            Assert.That(composite.Attributes[0].Type.DataType, Is.EqualTo(DataType.Integer));
            Assert.That(composite.Definition, OptionIs.None);
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForRangeType_ReturnsRangeWithSubtype()
    {
        var range = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Range>("db_test_udt_range_1");

        Assert.That(range.Subtype.TypeName.LocalName, Is.EqualTo("float8"));
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_ReturnsEachTestTypeAsItsCase()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var typesByName = types
            .Where(static t => t.Name.LocalName.StartsWith("db_test_udt_", StringComparison.Ordinal))
            .ToDictionary(static t => t.Name.LocalName, static t => t.Value, StringComparer.Ordinal);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typesByName["db_test_udt_domain_1"], Is.TypeOf<DatabaseUserDefinedType.Domain>());
            Assert.That(typesByName["db_test_udt_domain_2"], Is.TypeOf<DatabaseUserDefinedType.Domain>());
            Assert.That(typesByName["db_test_udt_enum_1"], Is.TypeOf<DatabaseUserDefinedType.Enum>());
            Assert.That(typesByName["db_test_udt_composite_1"], Is.TypeOf<DatabaseUserDefinedType.Composite>());
            Assert.That(typesByName["db_test_udt_range_1"], Is.TypeOf<DatabaseUserDefinedType.Range>());
            Assert.That(typesByName, Does.Not.ContainKey("db_test_udt_table_1"));
        }
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenLocalNameOnly_ReturnsTypeWithQualifiedName()
    {
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_domain_1");

        var type = await TypeProvider.GetUserDefinedType("db_test_udt_domain_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Value, Is.TypeOf<DatabaseUserDefinedType.Domain>());
        var domain = (DatabaseUserDefinedType.Domain)type.Value!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Name, Is.EqualTo(expectedTypeName));
            Assert.That(domain.Checks, Has.Count.EqualTo(1));
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
