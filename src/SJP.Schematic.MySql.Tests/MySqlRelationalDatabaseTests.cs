using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlRelationalDatabaseTests
{
    private static IRelationalDatabase Database
    {
        get
        {
            var connection = Mock.Of<ISchematicConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new MySqlRelationalDatabase(connection, identifierDefaults);
        }
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new MySqlRelationalDatabase(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();

        Assert.That(() => new MySqlRelationalDatabase(connection, null), Throws.ArgumentNullException);
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

    [Test]
    public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetRoutine(null), Throws.ArgumentNullException);
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
        public static async Task GetAllSequences_WhenRetrieved_ContainsNoValues()
        {
            var sequences = await Database.GetAllSequences().ConfigureAwait(false);

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
            var synonyms = await Database.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);

            Assert.That(synonyms, Is.Empty);
        }

        [Test]
        public static async Task GetAllSynonyms2_WhenRetrieved_ContainsNoValues()
        {
            var synonyms = await Database.GetAllSynonyms2().ConfigureAwait(false);

            Assert.That(synonyms, Is.Empty);
        }
    }
}