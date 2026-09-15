using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

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

        Assert.That(
            () => new PostgreSqlRelationalDatabase(null, identifierDefaults, identifierResolver),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(
            () => new PostgreSqlRelationalDatabase(connection, null, identifierResolver),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(
            () => new PostgreSqlRelationalDatabase(connection, identifierDefaults, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver")
        );
    }

    [Test]
    public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Database.GetTable(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Database.GetView(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName")
        );
    }

    [Test]
    public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Database.GetSequence(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }

    [Test]
    public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Database.GetSynonym(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymName")
        );
    }

    [Test]
    public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => Database.GetRoutine(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineName")
        );
    }

    // testing that the behaviour is equivalent to an empty synonym provider
    internal static class SynonymTests
    {
        [Test]
        public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
        {
            Assert.That(
            () => Database.GetSynonym(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymName")
        );
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