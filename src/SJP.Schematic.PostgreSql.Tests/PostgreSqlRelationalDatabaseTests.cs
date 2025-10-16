using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class PostgreSqlRelationalDatabaseTests
{
    private static IRelationalDatabase Database
    {
        get
        {
            var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();
            var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

            return new PostgreSqlRelationalDatabase(connection, identifierDefaults, identifierResolver);
        }
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(() => new PostgreSqlRelationalDatabase(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(() => new PostgreSqlRelationalDatabase(connection, null, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlRelationalDatabase(connection, identifierDefaults, null), Throws.ArgumentNullException);
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
    public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetSequence(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetSynonym(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetRoutine(null), Throws.ArgumentNullException);
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
            var synonymIsNone = await Database.GetSynonym(synonymName).IsNone;

            Assert.That(synonymIsNone, Is.True);
        }

        [Test]
        public static async Task EnumerateAllSynonyms_WhenEnumerated_ContainsNoValues()
        {
            var hasSynonyms = await Database.EnumerateAllSynonyms().AnyAsync();

            Assert.That(hasSynonyms, Is.False);
        }

        [Test]
        public static async Task GetAllSynonyms_WhenRetrieved_ContainsNoValues()
        {
            var synonyms = await Database.GetAllSynonyms();

            Assert.That(synonyms, Is.Empty);
        }
    }
}