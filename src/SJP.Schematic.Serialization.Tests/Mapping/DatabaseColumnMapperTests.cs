using System;
using System.Collections.Generic;
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
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var result = mapper.Map(mapper.Map(column));

        Assert.That(result.Default, OptionIs.None);
    }

    [Test]
    public static void Map_GivenNamedLiteralDefault_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var defaultValue = new DatabaseDefaultValue.Literal("((0))", Option<Identifier>.Some("df_test_column"));
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.Some(defaultValue), new ColumnValueSource.Stored());

        var result = mapper.Map(mapper.Map(column)).Default.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.Literal>());
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.ConstraintName.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("df_test_column")));
        }
    }

    [Test]
    public static void Map_GivenSequenceDefault_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");
        var defaultValue = new DatabaseDefaultValue.SequenceNextValue(
            "nextval('test_schema.test_seq'::regclass)",
            Option<Identifier>.None,
            Option<Identifier>.Some(sequenceName)
        );
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.Some(defaultValue), new ColumnValueSource.Stored());

        var result = mapper.Map(mapper.Map(column)).Default.UnwrapSome();

        Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.SequenceNextValue>());
        var sequenceDefault = (DatabaseDefaultValue.SequenceNextValue)result.Value!;
        Assert.That(sequenceDefault.SequenceName.UnwrapSome(), Is.EqualTo(sequenceName));
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.Unknown>());
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.ConstraintName, OptionIs.None);
        }
    }

    private static IEnumerable<TestCaseData> DefaultValuesWithKinds()
    {
        yield return new TestCaseData((DatabaseDefaultValue)new DatabaseDefaultValue.Unknown("((0))"), Dto.DefaultValueKind.Unknown).SetArgDisplayNames("Unknown");
        yield return new TestCaseData((DatabaseDefaultValue)new DatabaseDefaultValue.Literal("((0))"), Dto.DefaultValueKind.Literal).SetArgDisplayNames("Literal");
        yield return new TestCaseData((DatabaseDefaultValue)new DatabaseDefaultValue.Null("(NULL)"), Dto.DefaultValueKind.Null).SetArgDisplayNames("Null");
        yield return new TestCaseData((DatabaseDefaultValue)new DatabaseDefaultValue.Expression("(getdate())"), Dto.DefaultValueKind.Expression).SetArgDisplayNames("Expression");
        yield return new TestCaseData((DatabaseDefaultValue)new DatabaseDefaultValue.SequenceNextValue("(next value for [test_seq])"), Dto.DefaultValueKind.SequenceNextValue).SetArgDisplayNames("SequenceNextValue");
    }

    [TestCaseSource(nameof(DefaultValuesWithKinds))]
    public static void Map_GivenDefaultOfEachKind_WritesKindAndDefinition(DatabaseDefaultValue defaultValue, Dto.DefaultValueKind expectedKind)
    {
        var mapper = new DatabaseColumnMapper();
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.Some(defaultValue), new ColumnValueSource.Stored());

        var result = mapper.Map(column);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.DefaultValueKind, Is.EqualTo(expectedKind));
            Assert.That(result.DefaultValue, Is.EqualTo(defaultValue.Definition));
            Assert.That(result.DefaultSequenceName, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenColumnWithoutDefault_WritesUnknownKindAndNoDefault()
    {
        var mapper = new DatabaseColumnMapper();
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var result = mapper.Map(column);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.DefaultValue, Is.Null);
            Assert.That(result.DefaultConstraintName, Is.Null);
            Assert.That(result.DefaultValueKind, Is.EqualTo(Dto.DefaultValueKind.Unknown));
            Assert.That(result.DefaultSequenceName, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenSequenceDefaultWithName_WritesSequenceName()
    {
        var mapper = new DatabaseColumnMapper();
        var defaultValue = new DatabaseDefaultValue.SequenceNextValue(
            "(next value for [test_schema].[test_seq])",
            Option<Identifier>.None,
            Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier("test_schema", "test_seq"))
        );
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.Some(defaultValue), new ColumnValueSource.Stored());

        var result = mapper.Map(column);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.DefaultSequenceName?.Schema, Is.EqualTo("test_schema"));
            Assert.That(result.DefaultSequenceName?.LocalName, Is.EqualTo("test_seq"));
        }
    }

    [TestCase(Dto.DefaultValueKind.Unknown, typeof(DatabaseDefaultValue.Unknown))]
    [TestCase(Dto.DefaultValueKind.Literal, typeof(DatabaseDefaultValue.Literal))]
    [TestCase(Dto.DefaultValueKind.Null, typeof(DatabaseDefaultValue.Null))]
    [TestCase(Dto.DefaultValueKind.Expression, typeof(DatabaseDefaultValue.Expression))]
    [TestCase(Dto.DefaultValueKind.SequenceNextValue, typeof(DatabaseDefaultValue.SequenceNextValue))]
    public static void Map_GivenSerializedDefaultOfEachKind_ReadsBackAsMatchingCase(Dto.DefaultValueKind kind, Type expectedType)
    {
        var mapper = new DatabaseColumnMapper();
        var dto = CreateDefaultedColumnDto(kind, "((0))", Option<Identifier>.Some("df_test_column"), Option<Identifier>.None);

        var result = mapper.Map(dto).Default.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf(expectedType));
            Assert.That(result.Definition, Is.EqualTo("((0))"));
            Assert.That(result.ConstraintName.UnwrapSome().LocalName, Is.EqualTo("df_test_column"));
        }
    }

    [Test]
    public static void Map_GivenSerializedSequenceDefaultWithName_ReadsBackSequenceName()
    {
        var mapper = new DatabaseColumnMapper();
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");
        var dto = CreateDefaultedColumnDto(Dto.DefaultValueKind.SequenceNextValue, "(next value for [test_schema].[test_seq])", Option<Identifier>.None, Option<Identifier>.Some(sequenceName));

        var result = mapper.Map(dto).Default.UnwrapSome();

        Assert.That(result.Value, Is.TypeOf<DatabaseDefaultValue.SequenceNextValue>());
        var sequenceDefault = (DatabaseDefaultValue.SequenceNextValue)result.Value!;
        Assert.That(sequenceDefault.SequenceName.UnwrapSome(), Is.EqualTo(sequenceName));
    }

    [TestCase(Dto.DefaultValueKind.Unknown)]
    [TestCase(Dto.DefaultValueKind.Literal)]
    [TestCase(Dto.DefaultValueKind.Null)]
    [TestCase(Dto.DefaultValueKind.Expression)]
    public static void Map_GivenSerializedNonSequenceDefaultWithSequenceName_DropsSequenceName(Dto.DefaultValueKind kind)
    {
        var mapper = new DatabaseColumnMapper();
        var dto = CreateDefaultedColumnDto(kind, "0", Option<Identifier>.None, Option<Identifier>.Some("test_seq"));

        var result = mapper.Map(mapper.Map(dto));

        Assert.That(result.DefaultSequenceName, Is.Null);
    }

    [Test]
    public static void Map_GivenSerializedDefaultWithInvalidKind_ThrowsArgumentException()
    {
        var mapper = new DatabaseColumnMapper();
        var dto = CreateDefaultedColumnDto((Dto.DefaultValueKind)55, "0", Option<Identifier>.None, Option<Identifier>.None);

        Assert.That(
            () => mapper.Map(dto),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void Map_GivenSerializedKindWithoutDefault_ReturnsNoneDefault()
    {
        var mapper = new DatabaseColumnMapper();
        var dto = CreateDefaultedColumnDto(Dto.DefaultValueKind.Literal, null, Option<Identifier>.None, Option<Identifier>.None);

        var result = mapper.Map(dto);

        Assert.That(result.Default, OptionIs.None);
    }

    private static Dto.DatabaseColumn CreateDefaultedColumnDto(Dto.DefaultValueKind kind, string definition, Option<Identifier> constraintName, Option<Identifier> sequenceName)
    {
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        return new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map((Identifier)"test_column"),
            Type = dbTypeMapper.Map(ColumnType),
            IsNullable = true,
            DefaultValue = definition,
            DefaultConstraintName = identifierMapper.Map(constraintName),
            DefaultValueKind = kind,
            DefaultSequenceName = identifierMapper.Map(sequenceName),
        };
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
            Option<DatabaseDefaultValue>.None,
            new ColumnValueSource.Stored(),
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
