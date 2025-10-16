using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlDatabaseSequenceProviderTests : PostgreSqlTest
{
    private IDatabaseSequenceProvider SequenceProvider => new PostgreSqlDatabaseSequenceProvider(Connection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_1", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_1", CancellationToken.None);
    }

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
}