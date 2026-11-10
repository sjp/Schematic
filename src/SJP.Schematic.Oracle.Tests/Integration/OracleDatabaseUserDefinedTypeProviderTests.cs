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
        "create type db_test_udt_object_1 as object (attr_one number(10, 2), attr_two varchar2(50))",
        "create type db_test_udt_varray_1 as varray(10) of number(9)",
        "create type db_test_udt_nested_1 as table of varchar2(30)",
        "create type db_test_udt_object_2 as object (number_attr number, integer_attr integer, nvarchar_attr nvarchar2(100), char_semantics_attr varchar2(10 char), byte_semantics_attr varchar2(100 byte))",
        "create type db_test_udt_nested_2 as table of varchar2(10 char)",
        "begin dbms_ddl.create_wrapped('create type db_test_udt_wrapped_1 as object (attr_one number(10, 2))'); end;"
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop type db_test_udt_object_1",
        "drop type db_test_udt_varray_1",
        "drop type db_test_udt_nested_1",
        "drop type db_test_udt_object_2",
        "drop type db_test_udt_nested_2",
        "drop type db_test_udt_wrapped_1"
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

        Assert.That(type.Value, Is.TypeOf<DatabaseUserDefinedType.Composite>());
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsAttributesInDeclarationOrder()
    {
        var expectedAttributeNames = new[] { "ATTR_ONE", "ATTR_TWO" };

        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("DB_TEST_UDT_OBJECT_1");
        var attributeNames = type.Attributes.Select(static a => a.Name.LocalName).ToList();

        Assert.That(attributeNames, Is.EqualTo(expectedAttributeNames));
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsAttributesWithCorrectTypes()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("DB_TEST_UDT_OBJECT_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Attributes[0].Type.TypeName.LocalName, Is.EqualTo("NUMBER"));
            Assert.That(type.Attributes[0].Type.NumericPrecision.UnwrapSome().Precision, Is.EqualTo(10));
            Assert.That(type.Attributes[0].Type.NumericPrecision.UnwrapSome().Scale, Is.EqualTo(2));
            Assert.That(type.Attributes[1].Type.TypeName.LocalName, Is.EqualTo("VARCHAR2"));
            Assert.That(type.Attributes[1].Type.MaxLength, Is.EqualTo(50));
        }
    }

    // ALL_TYPE_ATTRS reports INTEGER by name, and a length declared in characters in characters
    [TestCase(0, "\"NUMBER\"", DataType.Numeric)]
    [TestCase(1, "\"INTEGER\"", DataType.BigInteger)]
    [TestCase(2, "\"NVARCHAR2\"(100)", DataType.Unicode)]
    [TestCase(3, "\"VARCHAR2\"(10 CHAR)", DataType.String)]
    [TestCase(4, "\"VARCHAR2\"(100)", DataType.String)]
    public async Task GetUserDefinedType_ForObjectTypeWithLengthOrPrecisionAttribute_ReturnsDeclaredType(int attributeIndex, string expectedDefinition, DataType expectedDataType)
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("DB_TEST_UDT_OBJECT_2");
        var attributeType = type.Attributes[attributeIndex].Type;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributeType.Definition, Is.EqualTo(expectedDefinition));
            Assert.That(attributeType.DataType, Is.EqualTo(expectedDataType));
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForCollectionOfCharacterSemanticsElement_ReturnsDeclaredElementType()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Collection>("DB_TEST_UDT_NESTED_2");
        var baseType = type.ElementType;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(baseType.Definition, Is.EqualTo("\"VARCHAR2\"(10 CHAR)"));
            Assert.That(baseType.MaxLength, Is.EqualTo(10));
        }
    }

    // ALL_TYPE_ATTRS records no nullability rule for an attribute, so every attribute is reported
    // as nullable, and an object type declares no default either.
    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsNullableAttributesWithoutDefaults()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("DB_TEST_UDT_OBJECT_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.Attributes.Select(static a => a.IsNullable), Is.All.True);
            Assert.That(type.Attributes.Select(static a => a.Default.IsNone), Is.All.True);
        }
    }

    [Test]
    public async Task GetUserDefinedType_ForObjectType_ReturnsSpecification()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("DB_TEST_UDT_OBJECT_1");

        Assert.That(type.Definition.UnwrapSome(), Does.Contain("db_test_udt_object_1").IgnoreCase);
    }

    [Test]
    public async Task GetUserDefinedType_ForWrappedObjectType_ReturnsUnwrappedSpecification()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Composite>("DB_TEST_UDT_WRAPPED_1");

        Assert.That(type.Definition.UnwrapSome(), Is.EqualTo(ExpectedWrappedTypeSpecification));
    }

    [Test]
    public async Task GetAllUserDefinedTypes_ForWrappedObjectType_ReturnsUnwrappedSpecification()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var wrappedType = AsCase<DatabaseUserDefinedType.Composite>(types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_WRAPPED_1", StringComparison.Ordinal)));

        Assert.That(wrappedType.Definition.UnwrapSome(), Is.EqualTo(ExpectedWrappedTypeSpecification));
    }

    [Test]
    public async Task GetUserDefinedType_ForVarrayType_ReturnsCollectionWithElementType()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Collection>("DB_TEST_UDT_VARRAY_1");

        Assert.That(type.ElementType.TypeName.LocalName, Is.EqualTo("NUMBER"));
    }

    [Test]
    public async Task GetUserDefinedType_ForNestedTableType_ReturnsCollectionWithElementType()
    {
        var type = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Collection>("DB_TEST_UDT_NESTED_1");

        Assert.That(type.ElementType.TypeName.LocalName, Is.EqualTo("VARCHAR2"));
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
        var objectType = AsCase<DatabaseUserDefinedType.Composite>(types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_OBJECT_1", StringComparison.Ordinal)));
        var varrayType = AsCase<DatabaseUserDefinedType.Collection>(types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_VARRAY_1", StringComparison.Ordinal)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectType.Attributes.Select(static a => a.Name.LocalName), Is.EqualTo(new[] { "ATTR_ONE", "ATTR_TWO" }));
            Assert.That(objectType.Definition.UnwrapSome(), Does.Contain("db_test_udt_object_1").IgnoreCase);
            Assert.That(varrayType.Definition.UnwrapSome(), Does.Contain("db_test_udt_varray_1").IgnoreCase);
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypes_WhenRetrieved_MatchesTypeRetrievedIndividually()
    {
        var types = await TypeProvider.GetAllUserDefinedTypes(TestContext.CurrentContext.CancellationToken);
        var fromAll = AsCase<DatabaseUserDefinedType.Collection>(types.First(static t => string.Equals(t.Name.LocalName, "DB_TEST_UDT_VARRAY_1", StringComparison.Ordinal)));
        var fromSingle = await GetUserDefinedTypeAsync<DatabaseUserDefinedType.Collection>("DB_TEST_UDT_VARRAY_1");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromAll.Name, Is.EqualTo(fromSingle.Name));
            Assert.That(fromAll.ElementType.TypeName, Is.EqualTo(fromSingle.ElementType.TypeName));
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

    // the source that wrapping recovers is the normalized text Oracle compiled, not the text as typed
    private const string ExpectedWrappedTypeSpecification = "TYPE db_test_udt_wrapped_1 AS OBJECT (ATTR_ONE NUMBER(10, 2))";
}
