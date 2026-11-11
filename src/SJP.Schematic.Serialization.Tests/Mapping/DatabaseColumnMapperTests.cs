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

    [Test]
    public static void Map_GivenStoredColumn_WritesNoComputedOrIdentityMembers()
    {
        var mapper = new DatabaseColumnMapper();
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var dto = mapper.Map(column);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dto.IsComputed, Is.False);
            Assert.That(dto.Definition, Is.Null);
            Assert.That(dto.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Unknown));
            Assert.That(dto.AutoIncrement, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenComputedColumn_WritesComputedMembersOnly()
    {
        var mapper = new DatabaseColumnMapper();
        var valueSource = new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Stored);
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, valueSource);

        var dto = mapper.Map(column);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dto.IsComputed, Is.True);
            Assert.That(dto.Definition, Is.EqualTo("a + b"));
            Assert.That(dto.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Stored));
            Assert.That(dto.AutoIncrement, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenGeneratedColumn_WritesIdentityMembersOnly()
    {
        var mapper = new DatabaseColumnMapper();
        var valueSource = new ColumnValueSource.Generated(new AutoIncrement(10, 5));
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, valueSource);

        var dto = mapper.Map(column);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dto.IsComputed, Is.False);
            Assert.That(dto.Definition, Is.Null);
            Assert.That(dto.ComputedStorage, Is.EqualTo(ComputedColumnStorage.Unknown));
            Assert.That(dto.AutoIncrement?.InitialValue, Is.EqualTo(10));
            Assert.That(dto.AutoIncrement?.Increment, Is.EqualTo(5));
        }
    }

    [Test]
    public static void Map_GivenStoredColumn_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var result = mapper.Map(mapper.Map(column));

        Assert.That(result.ValueSource, Is.EqualTo((ColumnValueSource)new ColumnValueSource.Stored()));
    }

    [TestCase("a + b", ComputedColumnStorage.Stored)]
    [TestCase("a + b", ComputedColumnStorage.Virtual)]
    [TestCase("a + b", ComputedColumnStorage.Unknown)]
    [TestCase(null, ComputedColumnStorage.Virtual)]
    public static void Map_GivenComputedColumn_RoundTrips(string definition, ComputedColumnStorage storage)
    {
        var mapper = new DatabaseColumnMapper();
        var computedDefinition = definition != null ? Option<string>.Some(definition) : Option<string>.None;
        var valueSource = new ColumnValueSource.Computed(computedDefinition, storage);
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, valueSource);

        var result = mapper.Map(mapper.Map(column));

        Assert.That(result.ValueSource, Is.EqualTo((ColumnValueSource)valueSource));
    }

    [Test]
    public static void Map_GivenGeneratedColumn_RoundTrips()
    {
        var mapper = new DatabaseColumnMapper();
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_seq");
        var identity = new AutoIncrement(10, 5, IdentityGeneration.Always, Option<decimal>.Some(1), Option<decimal>.Some(1000), true, Option<Identifier>.Some(sequenceName));
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Generated(identity));

        var result = mapper.Map(mapper.Map(column));

        Assert.That(result.ValueSource.UnwrapIdentity(), Is.EqualTo(identity));
    }

    [Test]
    public static void Map_GivenStoredColumnWithDefault_RoundTripsBoth()
    {
        var mapper = new DatabaseColumnMapper();
        var defaultValue = new DatabaseDefaultValue.Literal("0");
        var column = new DatabaseColumn("test_column", ColumnType, true, Option<DatabaseDefaultValue>.Some(defaultValue), new ColumnValueSource.Stored());

        var result = mapper.Map(mapper.Map(column));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ValueSource.Value, Is.TypeOf<ColumnValueSource.Stored>());
            Assert.That(result.Default.UnwrapSome().Definition, Is.EqualTo("0"));
        }
    }

    [Test]
    public static void Map_GivenDocumentWithComputedAndIdentityColumn_ReadsBackAsComputed()
    {
        var mapper = new DatabaseColumnMapper();
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        // no database reports a column that is both, so only a hand-written document can say so
        var dto = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map((Identifier)"test_column"),
            Type = dbTypeMapper.Map(ColumnType),
            IsNullable = true,
            IsComputed = true,
            Definition = "a + b",
            ComputedStorage = ComputedColumnStorage.Stored,
            AutoIncrement = new Dto.AutoIncrement { InitialValue = 1, Increment = 1 },
        };

        var result = mapper.Map(dto);

        Assert.That(result.ValueSource, Is.EqualTo((ColumnValueSource)new ColumnValueSource.Computed(Option<string>.Some("a + b"), ComputedColumnStorage.Stored)));
    }

    [Test]
    public static void Map_GivenDocumentWithDefinitionOnNonComputedColumn_ReadsBackAsStored()
    {
        var mapper = new DatabaseColumnMapper();
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        var dto = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map((Identifier)"test_column"),
            Type = dbTypeMapper.Map(ColumnType),
            IsNullable = true,
            IsComputed = false,
            Definition = "a + b",
            ComputedStorage = ComputedColumnStorage.Stored,
        };

        var result = mapper.Map(dto);

        Assert.That(result.ValueSource, Is.EqualTo((ColumnValueSource)new ColumnValueSource.Stored()));
    }

    [Test]
    public static void Map_GivenDocumentWithInvalidComputedStorage_ThrowsArgumentException()
    {
        var mapper = new DatabaseColumnMapper();
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        var dto = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map((Identifier)"test_column"),
            Type = dbTypeMapper.Map(ColumnType),
            IsNullable = true,
            IsComputed = true,
            ComputedStorage = (ComputedColumnStorage)55,
        };

        Assert.That(() => mapper.Map(dto), Throws.ArgumentException);
    }
}
