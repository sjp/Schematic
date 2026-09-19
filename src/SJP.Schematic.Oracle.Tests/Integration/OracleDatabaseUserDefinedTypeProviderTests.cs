using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed class OracleDatabaseUserDefinedTypeProviderTests : OracleTest
{
    private IDatabaseUserDefinedTypeProvider TypeProvider => new OracleDatabaseUserDefinedTypeProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public Task Init() => ExecuteBatchAsync(
        "create type db_test_udt_object_1 as object (attr_one number(10, 2), attr_two varchar2(50))",
        "create type db_test_udt_varray_1 as varray(10) of number(9)",
        "create type db_test_udt_nested_1 as table of varchar2(30)"
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop type db_test_udt_object_1",
        "drop type db_test_udt_varray_1",
        "drop type db_test_udt_nested_1"
    );

    [Test]
    public async Task GetUserDefinedType_WhenTypePresent_ReturnsType()
    {
        var typeIsSome = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).IsSome;

        Assert.That(typeIsSome, Is.True);
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresent_ReturnsTypeWithCorrectName()
    {
        const string expectedTypeName = "DB_TEST_UDT_OBJECT_1";

        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Name.LocalName, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_UDT_OBJECT_1");

        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Name, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_UDT_OBJECT_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_UDT_OBJECT_1");

        var type = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Name, Is.EqualTo(expectedTypeName));
    }

    // The server and database a caller gives are never matched against, only reported back, so a
    // name qualified with a different server still resolves to the type in the connected database.
    [Test]
    public async Task GetUserDefinedType_WhenTypePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_UDT_OBJECT_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_UDT_OBJECT_1");

        var type = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Name, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedType_WhenTypeMissing_ReturnsNone()
    {
        var typeIsNone = await TypeProvider.GetUserDefinedType("db_test_udt_missing_1", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    // Oracle stores every identifier folded to upper case, and the type queries match strictly, so
    // a name given in the wrong case is not the name of an existing type.
    [Test]
    public async Task GetUserDefinedType_GivenNameInWrongCase_ReturnsNone()
    {
        var typeName = new Identifier(IdentifierDefaults.Schema, "db_test_udt_object_1");

        var typeIsNone = await TypeProvider.GetUserDefinedType(typeName, TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsCompositeKind()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Kind, Is.EqualTo(UserDefinedTypeKind.Composite));
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsAttributesInDeclarationOrder()
    {
        var expectedAttributeNames = new[] { "ATTR_ONE", "ATTR_TWO" };

        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var attributeNames = type.Attributes.Select(static a => a.Name.LocalName).ToList();

        Assert.That(attributeNames, Is.EqualTo(expectedAttributeNames));
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsAttributesWithCorrectTypes()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Attributes[0].Type.TypeName.LocalName, Is.EqualTo("NUMBER"));
            Assert.That(type.Attributes[0].Type.NumericPrecision.UnwrapSome().Precision, Is.EqualTo(10));
            Assert.That(type.Attributes[0].Type.NumericPrecision.UnwrapSome().Scale, Is.EqualTo(2));
            Assert.That(type.Attributes[1].Type.TypeName.LocalName, Is.EqualTo("VARCHAR2"));
            Assert.That(type.Attributes[1].Type.MaxLength, Is.EqualTo(50));
        }
    }

    // ALL_TYPE_ATTRS records no nullability rule for an attribute, so every attribute is reported
    // as nullable, and an object type declares no default either.
    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsNullableAttributesWithoutDefaults()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Attributes.Select(static a => a.IsNullable), Is.All.True);
            Assert.That(type.Attributes.Select(static a => a.Default.IsNone), Is.All.True);
        }
    }

    // An object type is not defined over another type; only a collection type is.
    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsNoBaseType()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.BaseType, OptionIs.None);
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsSpecification()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Definition.UnwrapSome(), Does.Contain("db_test_udt_object_1").IgnoreCase);
    }

    [Test]
    public async Task GetUserDefinedType_ForVarrayType_ReturnsCollectionKindWithElementTypeAsBaseType()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_VARRAY_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Kind, Is.EqualTo(UserDefinedTypeKind.Collection));
            Assert.That(type.BaseType.UnwrapSome().TypeName.LocalName, Is.EqualTo("NUMBER"));
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForNestedTableType_ReturnsCollectionKindWithElementTypeAsBaseType()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_NESTED_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Kind, Is.EqualTo(UserDefinedTypeKind.Collection));
            Assert.That(type.BaseType.UnwrapSome().TypeName.LocalName, Is.EqualTo("VARCHAR2"));
        }
    }

    // A collection type has no named attributes, so nothing is reported against it even though the
    // attribute query is issued for every type.
    [Test]
    public async Task GetUserDefinedType_ForCollectionType_ReturnsNoAttributes()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_VARRAY_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(type.Attributes, Is.Empty);
    }

    // Oracle constrains an object type through the methods declared on it, never through a check
    // constraint, and every Oracle type is nullable.
    [Test]
    public async Task GetUserDefinedType_WhenTypePresent_ReturnsNullableTypeWithoutChecksOrEnumValuesOrDefault()
    {
        var type = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.IsNullable, Is.True);
            Assert.That(type.Checks, Is.Empty);
            Assert.That(type.EnumValues, Is.Empty);
            Assert.That(type.DefaultValue, OptionIs.None);
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_ContainsTestTypes()
    {
        var expectedTypeNames = new[] { "DB_TEST_UDT_NESTED_1", "DB_TEST_UDT_OBJECT_1", "DB_TEST_UDT_VARRAY_1" };

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
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_UDT_OBJECT_1");

        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var objectType = types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_OBJECT_1", StringComparison.Ordinal));

        Assert.That(objectType.Name, Is.EqualTo(expectedTypeName));
    }

    // The attributes and specifications of every type are read in bulk and matched back to the type
    // they belong to, so each type must be given its own rows and no others.
    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_AttachesAttributesAndSpecificationToOwningType()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var objectType = types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_OBJECT_1", StringComparison.Ordinal));
        var varrayType = types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_VARRAY_1", StringComparison.Ordinal));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectType.Attributes.Select(static a => a.Name.LocalName), Is.EqualTo(new[] { "ATTR_ONE", "ATTR_TWO" }));
            Assert.That(objectType.Definition.UnwrapSome(), Does.Contain("db_test_udt_object_1").IgnoreCase);
            Assert.That(varrayType.Attributes, Is.Empty);
            Assert.That(varrayType.Definition.UnwrapSome(), Does.Contain("db_test_udt_varray_1").IgnoreCase);
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_MatchesTypeRetrievedIndividually()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var fromAll = types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_VARRAY_1", StringComparison.Ordinal));
        var fromSingle = await TypeProvider.GetUserDefinedType("DB_TEST_UDT_VARRAY_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromAll.Name, Is.EqualTo(fromSingle.Name));
            Assert.That(fromAll.Kind, Is.EqualTo(fromSingle.Kind));
            Assert.That(fromAll.BaseType.UnwrapSome().TypeName, Is.EqualTo(fromSingle.BaseType.UnwrapSome().TypeName));
            Assert.That(fromAll.Definition.UnwrapSome(), Is.EqualTo(fromSingle.Definition.UnwrapSome()));
        }
    }

    [Test]
    public async Task EnumerateAllUserDefinedTypes_WhenEnumerated_ContainsTestType()
    {
        const string expectedTypeName = "DB_TEST_UDT_OBJECT_1";

        var containsTestType = await TypeProvider.EnumerateAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken)
            .AnyAsync(t => string.Equals(t.Name.LocalName, expectedTypeName, StringComparison.Ordinal));

        Assert.That(containsTestType, Is.True);
    }

    // The definition query also confirms the type exists, so a hit costs the definition query plus
    // the attribute and specification queries, and a miss costs only the definition query.
    [Test]
    public async Task GetUserDefinedType_WhenTypePresent_IssuesNoSeparateNameQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var typeProvider = new OracleDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsSome = await typeProvider.GetUserDefinedType("DB_TEST_UDT_OBJECT_1", TestContext.CurrentContext.CancellationToken).IsSome;

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
        var typeProvider = new OracleDatabaseUserDefinedTypeProvider(countingConnectionFactory, IdentifierDefaults);

        var typeIsNone = await typeProvider.GetUserDefinedType("DB_TEST_UDT_MISSING_1", TestContext.CurrentContext.CancellationToken).IsNone;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsNone, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }
}
