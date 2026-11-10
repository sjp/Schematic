using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Queries;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerUserDefinedTypeMapperTests
{
    private static readonly Identifier TypeName = Identifier.CreateQualifiedIdentifier("dbo", "test_type");

    private static GetAllUserDefinedTypeDefinitions.Result CreateRow(bool isTableType = false, bool isAssemblyType = false) => new()
    {
        SchemaName = "dbo",
        TypeName = "test_type",
        IsTableType = isTableType,
        IsAssemblyType = isAssemblyType,
        IsNullable = false,
        MaxLength = 50,
        Precision = 0,
        Scale = 0,
        Collation = null,
        BaseTypeName = isTableType || isAssemblyType ? null : "varchar",
        AssemblyName = isAssemblyType ? "TestAssembly" : null,
        AssemblyClass = isAssemblyType ? "TestNamespace.TestType" : null,
        DefaultValue = "create default test_default as 'abc'",
    };

    private static T MapType<T>(GetAllUserDefinedTypeDefinitions.Result row, IDatabaseColumn[] attributes = null, IDatabaseCheckConstraint[] checks = null)
    {
        var result = SqlServerUserDefinedTypeMapper.MapType(TypeName, row, attributes ?? [], checks ?? [], new SqlServerDbTypeProvider());

        Assert.That(result.Value, Is.TypeOf<T>());
        return (T)result.Value!;
    }

    [Test]
    public static void MapType_GivenAliasRow_ReturnsAliasWithAliasedTypeNullabilityAndBoundDefault()
    {
        var alias = MapType<DatabaseUserDefinedType.Alias>(CreateRow());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(alias.Name, Is.EqualTo(TypeName));
            Assert.That(alias.AliasedType.TypeName, Is.EqualTo(Identifier.CreateQualifiedIdentifier("sys", "varchar")));
            Assert.That(alias.AliasedType.MaxLength, Is.EqualTo(50));
            Assert.That(alias.IsNullable, Is.False);
            Assert.That(alias.DefaultValue.UnwrapSome(), Is.EqualTo("create default test_default as 'abc'"));
        }
    }

    [Test]
    public static void MapType_GivenAliasRowWithoutBuiltInType_ReturnsUnknown()
    {
        MapType<DatabaseUserDefinedType.Unknown>(CreateRow() with { BaseTypeName = null });
    }

    [Test]
    public static void MapType_GivenTableTypeRow_ReturnsTableWithColumnsAndChecks()
    {
        var column = new DatabaseColumn("col", TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, Option<IAutoIncrement>.None);
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "([col] > (0))", true);

        var table = MapType<DatabaseUserDefinedType.Table>(CreateRow(isTableType: true), [column], [check]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Columns.Single(), Is.SameAs(column));
            Assert.That(table.Checks.Single(), Is.SameAs(check));
        }
    }

    [Test]
    public static void MapType_GivenAssemblyTypeRow_ReturnsClrWithAssemblyAndClassAsExternalName()
    {
        var clr = MapType<DatabaseUserDefinedType.Clr>(CreateRow(isAssemblyType: true));

        Assert.That(clr.ExternalName.UnwrapSome(), Is.EqualTo("TestAssembly.TestNamespace.TestType"));
    }

    [Test]
    public static void MapType_GivenAssemblyTypeRowWithoutVisibleAssembly_ReturnsClrWithoutExternalName()
    {
        var clr = MapType<DatabaseUserDefinedType.Clr>(CreateRow(isAssemblyType: true) with { AssemblyName = null, AssemblyClass = null });

        Assert.That(clr.ExternalName, OptionIs.None);
    }
}
