using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;

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
            3,
            20,
            Option<decimal>.Some(0),
            Option<decimal>.Some(100),
            true,
            2
        );
        var sequences = new[] { sequence };

        var result = dbContextBuilder.Generate(tables, views, sequences);

        Assert.That(result, Is.EqualTo(ExpectedSequenceTestResult).IgnoreLineEndingFormat);
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

    private static IRelationalDatabaseTable CreateTable(Identifier tableName) =>
        new RelationalDatabaseTable(tableName, [], Option<IDatabaseKey>.None, [], [], [], [], [], []);

    private const string ExpectedSequenceTestResult = """
using System;
using Microsoft.EntityFrameworkCore;

namespace test
{
    public class AppContext : DbContext
    {
        /// <summary>
        /// Configure the model that was discovered by convention from the defined entity types.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<decimal>("test_sequence").StartsAt(3M).IncrementsBy(20M);
        }
    }
}
""";
}