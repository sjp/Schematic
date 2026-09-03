using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests;

[TestFixture]
internal static class EFCoreDbContextBuilderTests
{
    [Test]
    public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
    {
        Assert.That(() => new EFCoreDbContextBuilder(null, "test"), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentException(string ns)
    {
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new EFCoreDbContextBuilder(nameTranslator, ns), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceContextClassName_ThrowsArgumentException(string className)
    {
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new EFCoreDbContextBuilder(nameTranslator, "test", className), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void ContextClassName_WhenNotProvidedInCtor_IsDefaultContextClassName()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        Assert.That(dbContextBuilder.ContextClassName, Is.EqualTo(EFCoreDbContextBuilder.DefaultContextClassName));
    }

    [Test]
    public static void Generate_GivenNullTables_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();

        Assert.That(() => dbContextBuilder.Generate(null, views, sequences), Throws.ArgumentNullException);
    }

    [Test]
    public static void Generate_GivenNullViews_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var sequences = Array.Empty<IDatabaseSequence>();

        Assert.That(() => dbContextBuilder.Generate(tables, null, sequences), Throws.ArgumentNullException);
    }

    [Test]
    public static void Generate_GivenNullSequences_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();

        Assert.That(() => dbContextBuilder.Generate(tables, views, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Generate_GivenValidSequence_ReturnsExpectedConfiguration()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();

        var sequence = new DatabaseSequence(
            "test_sequence",
            TestDbTypes.BigInteger,
            3,
            20,
            Option<decimal>.Some(0),
            Option<decimal>.Some(100),
            true,
            SequenceCacheMode.Sized,
            Option<int>.Some(2),
            true
        );
        var sequences = new[] { sequence };

        var result = dbContextBuilder.Generate(tables, views, sequences);

        Assert.That(result, Is.EqualTo(ExpectedSequenceTestResult).IgnoreLineEndingFormat);
    }

    [Test]
    public static void Generate_GivenCustomContextClassName_ReturnsExpectedConfiguration()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test", "MyContext");
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();

        var result = dbContextBuilder.Generate(tables, views, sequences);

        Assert.That(result, Is.EqualTo(ExpectedCustomClassNameTestResult).IgnoreLineEndingFormat);
    }

    [Test]
    public static void Generate_GivenSequenceWithNonIntegerValues_ReturnsIntegerConfiguration()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();

        var sequence = new DatabaseSequence(
            "test_sequence",
            TestDbTypes.BigInteger,
            decimal.MaxValue,
            1.5M,
            Option<decimal>.None,
            Option<decimal>.None,
            true,
            SequenceCacheMode.Sized,
            Option<int>.Some(2),
            true
        );
        var sequences = new[] { sequence };

        var result = dbContextBuilder.Generate(tables, views, sequences);

        Assert.That(result, Does.Contain("""modelBuilder.HasSequence<long>("test_sequence").StartsAt(9223372036854775807L).IncrementsBy(1).IsCyclic();"""));
    }

    [Test]
    public static void Generate_GivenObjectsWithTheSameNameInDifferentSchemas_GeneratesUniquelyNamedDbSets()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");
        var tables = new[]
        {
            CreateTable(Identifier.CreateQualifiedIdentifier("first", "test_object")),
            CreateTable(Identifier.CreateQualifiedIdentifier("second", "test_object")),
        };
        var views = new[]
        {
            new DatabaseView(Identifier.CreateQualifiedIdentifier("third", "test_object"), "select 1 as dummy", []),
        };
        var sequences = Array.Empty<IDatabaseSequence>();

        var result = dbContextBuilder.Generate(tables, views, sequences);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("DbSet<first.test_object> test_objects"));
            Assert.That(result, Does.Contain("DbSet<second.test_object> test_objects_1"));
            Assert.That(result, Does.Contain("DbSet<third.test_object> test_objects_2"));
        }
    }

    [Test]
    public static void Generate_GivenTwoForeignKeysToTheSameParent_ConfiguresDistinctNavigationProperties()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        var parentColumn = CreateColumn("address_id");
        var billingColumn = CreateColumn("billing_address_id");
        var shippingColumn = CreateColumn("shipping_address_id");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("address_pk"), DatabaseKeyType.Primary, [parentColumn], true);
        var billingRelationalKey = CreateRelationalKey("order", "billing_fk", billingColumn, "address", parentKey);
        var shippingRelationalKey = CreateRelationalKey("order", "shipping_fk", shippingColumn, "address", parentKey);

        var addressTable = new RelationalDatabaseTable(
            "address",
            [parentColumn],
            Option<IDatabaseKey>.Some(parentKey),
            [],
            [],
            [billingRelationalKey, shippingRelationalKey],
            [],
            [],
            []
        );
        var orderTable = new RelationalDatabaseTable(
            "order",
            [billingColumn, shippingColumn],
            Option<IDatabaseKey>.None,
            [],
            [billingRelationalKey, shippingRelationalKey],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([addressTable, orderTable], [], []);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("""HasOne(t => t.address).WithMany(t => t!.orders).HasForeignKey(t => t.billing_address_id)"""));
            Assert.That(result, Does.Contain("""HasOne(t => t.address_1).WithMany(t => t!.orders_1).HasForeignKey(t => t.shipping_address_id)"""));
        }
    }

    [Test]
    public static void Generate_GivenColumnNamedAfterParentTable_ConfiguresUniquelyNamedNavigationProperty()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        // the child's column property name is the same as the parent's class name, so the navigation is suffixed
        var childColumn = CreateColumn("address");
        var parentColumn = CreateColumn("address_id");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("address_pk"), DatabaseKeyType.Primary, [parentColumn], true);
        var relationalKey = CreateRelationalKey("order", "address_fk", childColumn, "address", parentKey);

        var addressTable = new RelationalDatabaseTable(
            "address",
            [parentColumn],
            Option<IDatabaseKey>.Some(parentKey),
            [],
            [],
            [relationalKey],
            [],
            [],
            []
        );
        var orderTable = new RelationalDatabaseTable(
            "order",
            [childColumn],
            Option<IDatabaseKey>.None,
            [],
            [relationalKey],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([addressTable, orderTable], [], []);

        Assert.That(result, Does.Contain("""HasOne(t => t.address_1).WithMany(t => t!.orders).HasForeignKey(t => t.address)"""));
    }

    [Test]
    public static void Generate_GivenUniqueChildKey_ConfiguresOneToOneRelationship()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        var childColumn = CreateColumn("address_id");
        var parentColumn = CreateColumn("address_id");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("address_pk"), DatabaseKeyType.Primary, [parentColumn], true);
        var childUniqueKey = new DatabaseKey(Option<Identifier>.Some("order_uk"), DatabaseKeyType.Unique, [childColumn], true);
        var relationalKey = CreateRelationalKey("order", "address_fk", childColumn, "address", parentKey);

        var addressTable = new RelationalDatabaseTable(
            "address",
            [parentColumn],
            Option<IDatabaseKey>.Some(parentKey),
            [],
            [],
            [relationalKey],
            [],
            [],
            []
        );
        var orderTable = new RelationalDatabaseTable(
            "order",
            [childColumn],
            Option<IDatabaseKey>.None,
            [childUniqueKey],
            [relationalKey],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([addressTable, orderTable], [], []);

        Assert.That(result, Does.Contain("""HasOne(t => t.address).WithOne(t => t!.orders).HasForeignKey<order>(t => t.address_id).HasPrincipalKey<address>(t => t!.address_id)"""));
    }

    [Test]
    public static void Generate_GivenParentColumnNamedAfterParentTable_TranslatesPrincipalKeyWithParentClassName()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        // the parent's column property name is suffixed because it matches the parent's class name
        var childColumn = CreateColumn("address_id");
        var parentColumn = CreateColumn("address");
        var parentKey = new DatabaseKey(Option<Identifier>.Some("address_pk"), DatabaseKeyType.Primary, [parentColumn], true);
        var relationalKey = CreateRelationalKey("order", "address_fk", childColumn, "address", parentKey);

        var addressTable = new RelationalDatabaseTable(
            "address",
            [parentColumn],
            Option<IDatabaseKey>.Some(parentKey),
            [],
            [],
            [relationalKey],
            [],
            [],
            []
        );
        var orderTable = new RelationalDatabaseTable(
            "order",
            [childColumn],
            Option<IDatabaseKey>.None,
            [],
            [relationalKey],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([addressTable, orderTable], [], []);

        Assert.That(result, Does.Contain("""HasForeignKey(t => t.address_id).HasPrincipalKey(t => t!.address_)"""));
    }

    [Test]
    public static void Generate_GivenColumnGeneratedByDefault_ConfiguresValueGeneratedOnAddOnly()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        var column = CreateIdentityColumn("address_id", IdentityGeneration.ByDefault);
        var table = new RelationalDatabaseTable(
            "address",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([table], [], []);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("""Property(t => t.address_id).ValueGeneratedOnAdd()"""));
            Assert.That(result, Does.Not.Contain("UseIdentityAlwaysColumn"));
        }
    }

    [Test]
    public static void Generate_GivenColumnGeneratedAlways_ConfiguresIdentityAlwaysColumn()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        var column = CreateIdentityColumn("address_id", IdentityGeneration.Always);
        var table = new RelationalDatabaseTable(
            "address",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([table], [], []);

        Assert.That(result, Does.Contain("""Property(t => t.address_id).ValueGeneratedOnAdd().UseIdentityAlwaysColumn()"""));
    }

    [Test]
    public static void Generate_GivenColumnWithoutIdentity_DoesNotConfigureValueGeneration()
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        var column = CreateColumn("address_id");
        var table = new RelationalDatabaseTable(
            "address",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([table], [], []);

        Assert.That(result, Does.Not.Contain("ValueGeneratedOnAdd"));
    }

    [TestCase(ComputedColumnStorage.Stored, """Property(t => t.address_id).HasComputedColumnSql("1 + 1", stored: true)""")]
    [TestCase(ComputedColumnStorage.Virtual, """Property(t => t.address_id).HasComputedColumnSql("1 + 1", stored: false)""")]
    [TestCase(ComputedColumnStorage.Unknown, """Property(t => t.address_id).HasComputedColumnSql("1 + 1")""")]
    public static void Generate_GivenComputedColumn_ConfiguresComputedColumnSql(ComputedColumnStorage storage, string expectedConfiguration)
    {
        var nameTranslator = new VerbatimNameTranslator();
        var dbContextBuilder = new EFCoreDbContextBuilder(nameTranslator, "test");

        var column = CreateComputedColumn("address_id", storage);
        var table = new RelationalDatabaseTable(
            "address",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        var result = dbContextBuilder.Generate([table], [], []);

        Assert.That(result, Does.Contain(expectedConfiguration));
    }

    private static IRelationalDatabaseTable CreateTable(Identifier tableName) =>
        new RelationalDatabaseTable(tableName, [], Option<IDatabaseKey>.None, [], [], [], [], [], []);

    private static IDatabaseColumn CreateColumn(Identifier columnName)
    {
        var columnType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(long),
            false,
            -1,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        return new DatabaseColumn(columnName, columnType, false, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);
    }

    private static IDatabaseColumn CreateComputedColumn(Identifier columnName, ComputedColumnStorage storage)
    {
        var columnType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(long),
            false,
            -1,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        return new DatabaseColumn(columnName, columnType, false, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None, true, Option<string>.Some("1 + 1"), storage);
    }

    private static IDatabaseColumn CreateIdentityColumn(Identifier columnName, IdentityGeneration generation)
    {
        var columnType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(long),
            false,
            -1,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var autoIncrement = new AutoIncrement(1, 1, generation, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.None);

        return new DatabaseColumn(columnName, columnType, false, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.Some(autoIncrement));
    }

    private static IDatabaseRelationalKey CreateRelationalKey(Identifier childTableName, Identifier childKeyName, IDatabaseColumn childColumn, Identifier parentTableName, IDatabaseKey parentKey) =>
        new DatabaseRelationalKey(
            childTableName,
            new DatabaseKey(Option<Identifier>.Some(childKeyName), DatabaseKeyType.Foreign, [childColumn], true),
            parentTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

    private const string ExpectedSequenceTestResult = """
using System;
using Microsoft.EntityFrameworkCore;

namespace test
{
    public class DatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <c>DatabaseContext</c> class.
        /// </summary>
        public DatabaseContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DatabaseContext</c> class.
        /// </summary>
        /// <param name="options">The options to be used by this context.</param>
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configure the model that was discovered by convention from the defined entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("test_sequence").StartsAt(3L).IncrementsBy(20).HasMin(0L).HasMax(100L).IsCyclic();
        }
    }
}
""";

    private const string ExpectedCustomClassNameTestResult = """
using System;
using Microsoft.EntityFrameworkCore;

namespace test
{
    public class MyContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <c>MyContext</c> class.
        /// </summary>
        public MyContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MyContext</c> class.
        /// </summary>
        /// <param name="options">The options to be used by this context.</param>
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configure the model that was discovered by convention from the defined entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
""";
}
