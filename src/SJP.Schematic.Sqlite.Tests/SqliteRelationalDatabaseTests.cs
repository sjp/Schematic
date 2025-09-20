using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteRelationalDatabaseTests
{
    private static ISqliteDatabase Database
    {
        get
        {
            var connection = Mock.Of<ISchematicConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var pragma = new ConnectionPragma(connection);

            return new SqliteRelationalDatabase(connection, identifierDefaults, pragma);
        }
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var pragma = new ConnectionPragma(Mock.Of<ISchematicConnection>());

        Assert.That(() => new SqliteRelationalDatabase(null, identifierDefaults, pragma), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var pragma = new ConnectionPragma(connection);

        Assert.That(() => new SqliteRelationalDatabase(connection, null, pragma), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullPragma_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteRelationalDatabase(connection, identifierDefaults, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetTable(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetView(null), Throws.ArgumentNullException);
    }

    // testing that the behaviour is equivalent to an empty sequence provider
    [TestFixture]
    internal static class SequenceTests
    {
        [Test]
        public static void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.GetSequence(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
        {
            var sequenceName = new Identifier("test");
            var sequenceIsNone = await Database.GetSequence(sequenceName).IsNone.ConfigureAwait(false);

            Assert.That(sequenceIsNone, Is.True);
        }

        [Test]
        public static async Task EnumerateAllSequences_WhenEnumerated_ContainsNoValues()
        {
            var hasSequences = await Database.EnumerateAllSequences()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasSequences, Is.False);
        }

        [Test]
        public static async Task GetAllSequences2_WhenRetrieved_ContainsNoValues()
        {
            var sequences = await Database.GetAllSequences2().ConfigureAwait(false);

            Assert.That(sequences, Is.Empty);
        }
    }

    // testing that the behaviour is equivalent to an empty synonym provider
    [TestFixture]
    internal static class SynonymTests
    {
        [Test]
        public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.GetSynonym(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
        {
            var synonymName = new Identifier("test");
            var synonymIsNone = await Database.GetSynonym(synonymName).IsNone.ConfigureAwait(false);

            Assert.That(synonymIsNone, Is.True);
        }

        [Test]
        public static async Task EnumerateAllSynonyms_WhenEnumerated_ContainsNoValues()
        {
            var hasSynonyms = await Database.EnumerateAllSynonyms()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasSynonyms, Is.False);
        }

        [Test]
        public static async Task GetAllSynonyms2_WhenRetrieved_ContainsNoValues()
        {
            var synonyms = await Database.GetAllSynonyms2().ConfigureAwait(false);

            Assert.That(synonyms, Is.Empty);
        }
    }

    // testing that the behaviour is equivalent to an empty routines provider
    [TestFixture]
    internal static class RoutineTests
    {
        [Test]
        public static void GetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
        {
            Assert.That(() => Database.GetRoutine(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task GetRoutine_GivenValidRoutineName_ReturnsNone()
        {
            var routineName = new Identifier("test");
            var routineIsNone = await Database.GetRoutine(routineName).IsNone.ConfigureAwait(false);

            Assert.That(routineIsNone, Is.True);
        }

        [Test]
        public static async Task EnumerateAllRoutines_WhenEnumerated_ContainsNoValues()
        {
            var hasRoutines = await Database.EnumerateAllRoutines()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasRoutines, Is.False);
        }

        [Test]
        public static async Task GetAllRoutines2_WhenRetrieved_ContainsNoValues()
        {
            var routiens = await Database.GetAllRoutines2().ConfigureAwait(false);

            Assert.That(routiens, Is.Empty);
        }
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void VacuumAsync_WhenGivenNullOrWhiteSpaceSchemaName_ThrowsArgumentException(string schemaName)
    {
        Assert.That(() => Database.VacuumAsync(schemaName), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void VacuumIntoAsync_WhenGivenNullOrWhiteSpaceFileName_ThrowsArgumentException(string fileName)
    {
        Assert.That(() => Database.VacuumIntoAsync(fileName), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void VacuumIntoAsync_WhenGivenFileNameWithNullSchemaName_ThrowsArgumentException(string schemaName)
    {
        Assert.That(() => Database.VacuumIntoAsync("test_file", schemaName), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void AttachDatabaseAsync_WhenGivenNullOrWhiteSpaceSchemaName_ThrowsArgumentException(string schemaName)
    {
        Assert.That(() => Database.AttachDatabaseAsync(schemaName, ":memory:"), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void AttachDatabaseAsync_WhenGivenNullOrWhiteSpaceFileName_ThrowsArgumentException(string fileName)
    {
        Assert.That(() => Database.AttachDatabaseAsync("test", fileName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void AttachDatabaseAsync_WhenGivenMainSchemaName_ThrowsArgumentException()
    {
        Assert.That(() => Database.AttachDatabaseAsync("main", ":memory:"), Throws.ArgumentException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void DetachDatabaseAsync_WhenGivenNullOrWhiteSpaceSchemaName_ThrowsArgumentException(string schemaName)
    {
        Assert.That(() => Database.DetachDatabaseAsync(schemaName), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void DetachDatabaseAsync_WhenGivenMainSchemaName_ThrowsArgumentException()
    {
        Assert.That(() => Database.DetachDatabaseAsync("main"), Throws.ArgumentException);
    }
}