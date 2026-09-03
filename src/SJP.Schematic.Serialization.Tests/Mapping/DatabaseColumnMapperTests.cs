using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseColumnMapperTests
{
    private static IDbType ColumnType { get; } = new ColumnDataType(
        "int",
        DataType.Integer,
        "int",
        typeof(int),
        false,
        4,
        Option<INumericPrecision>.None,
        Option<Identifier>.None
    );

    [Test]
    public static void Map_GivenColumnWithoutDefault_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);

        var result = mapper.Map(mapper.Map(column));

        Assert.That(result.Default, OptionIs.None);
    }

    [Test]
    public static void Map_GivenNamedLiteralDefault_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var defaultValue = new DatabaseDefaultValue(
            "((0))",
            DefaultValueKind.Literal,
            Option<Identifier>.Some("df_test_column"),
            Option<Identifier>.None
        );
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<IDatabaseDefaultValue>.Some(defaultValue), Option<IAutoIncrement>.None);

        var result = mapper.Map(mapper.Map(column)).Default.UnwrapSome();

        Assert.Multiple(() =>
        {
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.Kind, Is.EqualTo(DefaultValueKind.Literal));
            Assert.That(result.ConstraintName.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("df_test_column")));
            Assert.That(result.SequenceName, OptionIs.None);
        });
    }

    [Test]
    public static void Map_GivenSequenceDefault_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");
        var defaultValue = new DatabaseDefaultValue(
            "nextval('test_schema.test_seq'::regclass)",
            DefaultValueKind.SequenceNextValue,
            Option<Identifier>.None,
            Option<Identifier>.Some(sequenceName)
        );
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<IDatabaseDefaultValue>.Some(defaultValue), Option<IAutoIncrement>.None);

        var result = mapper.Map(mapper.Map(column)).Default.UnwrapSome();

        Assert.Multiple(() =>
        {
            Assert.That(result.Kind, Is.EqualTo(DefaultValueKind.SequenceNextValue));
            Assert.That(result.SequenceName.UnwrapSome(), Is.EqualTo(sequenceName));
        });
    }

    [Test]
    public static void Map_GivenDocumentWithoutDefaultClassification_ReadsBackAsUnknown()
    {
        var mapper = new DatabaseColumnMapper();
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        // a document written before defaults were classified carries the expression alone
        var dto = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map((Identifier)"test_column"),
            Type = dbTypeMapper.Map(ColumnType),
            IsNullable = true,
            DefaultValue = "((0))",
        };

        var result = mapper.Map(dto).Default.UnwrapSome();

        Assert.Multiple(() =>
        {
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.Kind, Is.EqualTo(DefaultValueKind.Unknown));
            Assert.That(result.ConstraintName, OptionIs.None);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void Map_GivenHiddenColumn_RoundTrips(bool isHidden)
    {
        var mapper = new DatabaseColumnMapper();
        var column = new DatabaseColumn(
            "test_column",
            ColumnType,
            true,
            Option<IDatabaseDefaultValue>.None,
            Option<IAutoIncrement>.None,
            false,
            Option<string>.None,
            ComputedColumnStorage.Unknown,
            isHidden
        );

        var result = mapper.Map(mapper.Map(column));

        Assert.That(result.IsHidden, Is.EqualTo(isHidden));
    }

    [Test]
    public static void Map_GivenDocumentWithoutHiddenColumns_ReadsBackAsVisible()
    {
        var mapper = new DatabaseColumnMapper();
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        // a document written before hidden columns were described says nothing about visibility
        var dto = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map((Identifier)"test_column"),
            Type = dbTypeMapper.Map(ColumnType),
            IsNullable = true,
        };

        var result = mapper.Map(dto);

        Assert.That(result.IsHidden, Is.False);
    }
}
