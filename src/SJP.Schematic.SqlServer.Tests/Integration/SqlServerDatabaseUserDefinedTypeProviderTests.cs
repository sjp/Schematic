using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed class SqlServerDatabaseUserDefinedTypeProviderTests : SqlServerTest
{
    private IDatabaseUserDefinedTypeProvider TypeProvider => new SqlServerDatabaseUserDefinedTypeProvider(DbConnection, IdentifierDefaults);

    private async Task<T> GetUserDefinedTypeAsync<T>(string typeName)
    {
        var type = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        return AsCase<T>(type);
    }

    private static T AsCase<T>(DatabaseUserDefinedType type)
    {
        Assert.That(type.Value, Is.TypeOf<T>());
        return (T)type.Value!;
    }

    [OneTimeSetUp]
    public Task Init() => ExecuteBatchAsync(
        "create type db_test_udt_alias_1 from varchar(50) not null",
        "create type db_test_udt_alias_2 from decimal(10, 5) null",
        // a table type names its constraints itself; the syntax accepts no constraint name
        """
        create type db_test_udt_table_1 as table (
            attr_one int not null identity(2, 3),
            attr_two varchar(50) null,
            attr_three as attr_one * 2,
            check (attr_one > 0)
        )
        """,
        // the seed is above the bigint range
        "create type db_test_udt_table_2 as table ( attr_one decimal(38, 0) not null identity(10000000000000000000, 1) )"
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop type db_test_udt_alias_1",
        "drop type db_test_udt_alias_2",
        "drop type db_test_udt_table_1",
        "drop type db_test_udt_table_2"
    );

    [Test]
    public async Task GetUserDefinedType_WhenTypePresent_ReturnsType()
    {
        var typeIsSome = await TypeProvider.GetUserDefinedType("db_test_udt_alias_1", TestContext.CurrentContext.CancellationToken).IsSome;

        Assert.That(typeIsSome, Is.True);
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_alias_1");

        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Alias>("db_test_udt_alias_1");

        Assert.That(type.Name, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier(IdentifierDefaults.Schema, "db_test_udt_alias_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_alias_1");

        var type = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Name, Is.EqualTo(expectedTypeName));
    }

    // The server and database a caller gives are never matched against, only reported back, so a
    // name qualified with a different server still resolves to the type in the connected database.
    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_alias_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_alias_1");

        var type = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Name, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypeMissing_ReturnsNone()
    {
        var typeIsNone = await TypeProvider.GetUserDefinedType("db_test_udt_missing_1", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    // sys.types holds every built-in type alongside the user-declared ones, and only the latter
    // are user-defined types.
    [Test]
    public async Task GetUserDefinedType_GivenBuiltInTypeName_ReturnsNone()
    {
        var typeName = new Identifier("sys", "varchar");

        var typeIsNone = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    [Test]
    public async Task GetUserDefinedType_ForAliasType_ReturnsAliasDefinedOverBuiltInType()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Alias>("db_test_udt_alias_1");
        var baseType = type.AliasedType;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(baseType.TypeName.Schema, Is.EqualTo("sys"));
            Assert.That(baseType.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(baseType.MaxLength, Is.EqualTo(50));
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForAliasTypeDeclaredNotNull_ReturnsNonNullableType()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Alias>("db_test_udt_alias_1");

        Assert.That(type.IsNullable, Is.False);
    }

    [Test]
    public async Task GetUserDefinedType_ForAliasTypeDeclaredNull_ReturnsNullableTypeWithNumericPrecision()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Alias>("db_test_udt_alias_2");
        var numericPrecision = type.AliasedType.NumericPrecision.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.IsNullable, Is.True);
            Assert.That(type.AliasedType.TypeName.LocalName, Is.EqualTo("decimal"));
            Assert.That(numericPrecision.Precision, Is.EqualTo(10));
            Assert.That(numericPrecision.Scale, Is.EqualTo(5));
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForAliasTypeWithoutBoundDefault_ReturnsNoDefault()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Alias>("db_test_udt_alias_1");

        Assert.That(type.DefaultValue, OptionIs.None);
    }

    [Test]
    public async Task GetUserDefinedType_ForTableType_ReturnsTable()
    {
        var type = await TypeProvider.GetUserDefinedType("db_test_udt_table_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Value, Is.TypeOf<DatabaseUserDefinedType.Table>());
    }

    [Test]
    public async Task GetUserDefinedType_ForTableType_ReturnsAttributesInDeclarationOrder()
    {
        var expectedAttributeNames = new[] { "attr_one", "attr_two", "attr_three" };

        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Table>("db_test_udt_table_1");
        var attributeNames = type.Columns.Select(static a => a.Name.LocalName).ToList();

        Assert.That(attributeNames, Is.EqualTo(expectedAttributeNames));
    }

    [Test]
    public async Task GetUserDefinedType_ForTableType_ReturnsAttributesWithCorrectTypesAndNullability()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Table>("db_test_udt_table_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Columns[0].Type.TypeName.LocalName, Is.EqualTo("int"));
            Assert.That(type.Columns[0].IsNullable, Is.False);
            Assert.That(type.Columns[1].Type.TypeName.LocalName, Is.EqualTo("varchar"));
            Assert.That(type.Columns[1].Type.MaxLength, Is.EqualTo(50));
            Assert.That(type.Columns[1].IsNullable, Is.True);
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForTableTypeWithIdentityAttribute_ReturnsSeedAndIncrement()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Table>("db_test_udt_table_1");
        var autoIncrement = type.Columns[0].AutoIncrement.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(autoIncrement.InitialValue, Is.EqualTo(2));
            Assert.That(autoIncrement.Increment, Is.EqualTo(3));
            Assert.That(type.Columns[1].AutoIncrement, OptionIs.None);
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForTableTypeWithIdentitySeedBeyondBigint_ReturnsSeedAndIncrement()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Table>("db_test_udt_table_2");
        var autoIncrement = type.Columns[0].AutoIncrement.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(autoIncrement.InitialValue, Is.EqualTo(10000000000000000000m));
            Assert.That(autoIncrement.Increment, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WithTableTypeWithIdentitySeedBeyondBigint_ReturnsSeed()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var tableType = AsCase<DatabaseUserDefinedType.Table>(types.First(static t => string.Equals(t.Name.LocalName, "db_test_udt_table_2", StringComparison.Ordinal)));
        var autoIncrement = tableType.Columns[0].AutoIncrement.UnwrapSome();

        Assert.That(autoIncrement.InitialValue, Is.EqualTo(10000000000000000000m));
    }

    [Test]
    public async Task GetUserDefinedType_ForTableTypeWithComputedAttribute_ReturnsComputedDefinition()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Table>("db_test_udt_table_1");
        var computedAttribute = type.Columns[2];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(computedAttribute.IsComputed, Is.True);
            Assert.That(computedAttribute.ComputedDefinition.UnwrapSome(), Does.Contain("attr_one"));
            Assert.That(computedAttribute.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Virtual));
            Assert.That(type.Columns[0].IsComputed, Is.False);
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForTableTypeWithCheck_ReturnsCheck()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Table>("db_test_udt_table_1");
        var check = type.Checks.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(check.Definition, Does.Contain("attr_one"));
            Assert.That(check.IsEnabled, Is.True);
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_ContainsTestTypes()
    {
        var expectedTypeNames = new[] { "db_test_udt_alias_1", "db_test_udt_alias_2", "db_test_udt_table_1" };

        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var typeNames = types
            .Select(static t => t.Name.LocalName)
            .Where(name => expectedTypeNames.Contains(name, StringComparer.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.That(typeNames, Is.EqualTo(expectedTypeNames));
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_ContainsTestTypeWithQualifiedName()
    {
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_udt_table_1");

        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var tableType = AsCase<DatabaseUserDefinedType.Table>(types.First(static t => string.Equals(t.Name.LocalName, "db_test_udt_table_1", StringComparison.Ordinal)));

        Assert.That(tableType.Name, Is.EqualTo(expectedTypeName));
    }

    // The attributes and checks of every table type are read in bulk and matched back to the type
    // they belong to, so each type must be given its own rows and no others.
    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_AttachesAttributesAndChecksToOwningType()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var tableType = AsCase<DatabaseUserDefinedType.Table>(types.First(static t => string.Equals(t.Name.LocalName, "db_test_udt_table_1", StringComparison.Ordinal)));
        var aliasType = types.First(static t => string.Equals(t.Name.LocalName, "db_test_udt_alias_1", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableType.Columns.Select(static a => a.Name.LocalName), Is.EqualTo(new[] { "attr_one", "attr_two", "attr_three" }));
            Assert.That(tableType.Checks, Has.Count.EqualTo(1));
            Assert.That(aliasType.Value, Is.TypeOf<DatabaseUserDefinedType.Alias>());
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_MatchesTypeRetrievedIndividually()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var fromAll = AsCase<DatabaseUserDefinedType.Alias>(types.First(static t => string.Equals(t.Name.LocalName, "db_test_udt_alias_1", StringComparison.Ordinal)));
        var fromSingle = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Alias>("db_test_udt_alias_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromAll.Name, Is.EqualTo(fromSingle.Name));
            Assert.That(fromAll.IsNullable, Is.EqualTo(fromSingle.IsNullable));
            Assert.That(fromAll.AliasedType.Definition, Is.EqualTo(fromSingle.AliasedType.Definition));
        }
    }

    [Test]
    public async Task EnumerateAllUserDefinedTypes_WhenEnumerated_ContainsTestType()
    {
        const string expectedTypeName = "db_test_udt_table_1";

        var containsTestType = await TypeProvider.EnumerateAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken)
            .AnyAsync(t => string.Equals(t.Name.LocalName, expectedTypeName, StringComparison.Ordinal));

        Assert.That(containsTestType, Is.True);
    }

    // Only a table type is described by more than its own row, so an alias type costs the single
    // definition query that also confirms it exists.
    [Test]
    public async Task GetUserDefinedType_WhenAliasTypePresent_IssuesOneQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var typeProvider = new SqlServerDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsSome = await typeProvider.GetUserDefinedType("db_test_udt_alias_1", TestContext.CurrentContext.CancellationToken).IsSome;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsSome, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task GetUserDefinedType_WhenTableTypePresent_IssuesDefinitionAttributeAndCheckQueries()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var typeProvider = new SqlServerDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsSome = await typeProvider.GetUserDefinedType("db_test_udt_table_1", TestContext.CurrentContext.CancellationToken).IsSome;

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
        var typeProvider = new SqlServerDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsNone = await typeProvider.GetUserDefinedType("db_test_udt_missing_1", TestContext.CurrentContext.CancellationToken).IsNone;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsNone, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }
}
