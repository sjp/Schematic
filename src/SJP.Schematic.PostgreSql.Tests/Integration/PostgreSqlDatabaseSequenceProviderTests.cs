using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseSequenceProviderTests : PostgreSqlTest
{
    private IDatabaseSequenceProvider SequenceProvider => new PostgreSqlDatabaseSequenceProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public Task Init() => ExecuteBatchAsync(
        "create sequence db_test_sequence_1",
        "create sequence \"DB_Test_Sequence_2\""
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop sequence db_test_sequence_1",
        "drop sequence \"DB_Test_Sequence_2\""
    );

    [Test]
    public async Task GetSequence_WhenSequencePresent_ReturnsSequence()
    {
        var sequenceIsSome = await SequenceProvider.GetSequence("db_test_sequence_1").IsSome;
        Assert.That(sequenceIsSome, Is.True);
    }

    [Test]
    public async Task GetSequence_WhenSequencePresent_ReturnsSequenceWithCorrectName()
    {
        const string sequenceName = "db_test_sequence_1";
        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name.LocalName, Is.EqualTo(sequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name, Is.EqualTo(sequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync();

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequenceMissing_ReturnsNone()
    {
        var sequenceIsNone = await SequenceProvider.GetSequence("sequence_that_doesnt_exist").IsNone;
        Assert.That(sequenceIsNone, Is.True);
    }

    [Test]
    public async Task EnumerateAllSequences_WhenEnumerated_ContainsSequences()
    {
        var hasSequences = await SequenceProvider.EnumerateAllSequences().AnyAsync();

        Assert.That(hasSequences, Is.True);
    }

    [Test]
    public async Task EnumerateAllSequences_WhenEnumerated_ContainsTestSequence()
    {
        var containsTestSequence = await SequenceProvider.EnumerateAllSequences()
            .AnyAsync(s => string.Equals(s.Name.LocalName, "db_test_sequence_1", StringComparison.Ordinal));

        Assert.That(containsTestSequence, Is.True);
    }

    [Test]
    public async Task GetAllSequences_WhenRetrieved_ContainsSequences()
    {
        var sequences = await SequenceProvider.GetAllSequences();

        Assert.That(sequences, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllSequences_WhenRetrieved_ContainsTestSequence()
    {
        var sequences = await SequenceProvider.GetAllSequences();
        var containsTestSequence = sequences.Any(s => string.Equals(s.Name.LocalName, "db_test_sequence_1", StringComparison.Ordinal));

        Assert.That(containsTestSequence, Is.True);
    }

    [Test]
    public async Task Type_GivenDefaultSequence_ReturnsBigInteger()
    {
        var sequence = await SequenceProvider.GetSequence("db_test_sequence_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequence.Type.TypeName.LocalName, Is.EqualTo("bigint"));
            Assert.That(sequence.Type.ClrType, Is.EqualTo(typeof(long)));
        }
    }

    [Test]
    public async Task CacheMode_GivenDefaultSequence_ReturnsSizedCache()
    {
        var sequence = await SequenceProvider.GetSequence("db_test_sequence_1").UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequence.CacheMode, Is.EqualTo(SequenceCacheMode.Sized));
            Assert.That(sequence.CacheSize.UnwrapSome(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task IsOrdered_GivenDefaultSequence_ReturnsTrue()
    {
        var sequence = await SequenceProvider.GetSequence("db_test_sequence_1").UnwrapSomeAsync();

        Assert.That(sequence.IsOrdered, Is.True);
    }

    // A name with no uppercase letters has a single resolution candidate, even when it contains
    // underscores or digits, so looking up a missing sequence costs one query.
    [Test]
    public async Task GetSequence_WhenLowercaseSequenceMissingInLowercaseSchema_IssuesOneQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var sequenceProvider = new PostgreSqlDatabaseSequenceProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        var sequenceIsNone = await sequenceProvider.GetSequence(new Identifier("missing_schema_1", "missing_sequence_1"), TestContext.CurrentContext.CancellationToken).IsNone;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequenceIsNone, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }

    // The definition query also confirms the sequence exists, so no separate name lookup is made.
    [Test]
    public async Task GetSequence_WhenSequencePresent_IssuesOneQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var sequenceProvider = new PostgreSqlDatabaseSequenceProvider(countingConnection, IdentifierDefaults, IdentifierResolver);

        var sequenceIsSome = await sequenceProvider.GetSequence("db_test_sequence_1", TestContext.CurrentContext.CancellationToken).IsSome;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequenceIsSome, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }

    // The lowercased candidate is tried first and misses, then the name as written matches.
    [Test]
    public async Task GetSequence_WhenQuotedMixedCaseSequencePresent_ReturnsSequenceAfterTryingLowercasedName()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var sequenceProvider = new PostgreSqlDatabaseSequenceProvider(countingConnection, IdentifierDefaults, IdentifierResolver);
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_Test_Sequence_2");

        var sequence = await sequenceProvider.GetSequence("DB_Test_Sequence_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(2));
        }
    }
}
