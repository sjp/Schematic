using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration;

internal sealed class SqlServerDatabaseSequenceProviderTests : SqlServerTest
{
    private IDatabaseSequenceProvider SequenceProvider => new SqlServerDatabaseSequenceProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_2 start with 20", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_3 start with 100 increment by 100", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_4 start with 1000 minvalue -99", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_5 start with 1000 no minvalue", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_6 start with 1 maxvalue 333", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_7 start with 1 no maxvalue", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_8 cycle", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_9 no cycle", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_10 cache 10", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_11 no cache", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_3", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_4", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_5", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_6", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_7", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_8", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_9", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_10", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop sequence db_test_sequence_11", CancellationToken.None).ConfigureAwait(false);
    }

    private Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return GetSequenceAsyncCore(sequenceName);
    }

    private async Task<IDatabaseSequence> GetSequenceAsyncCore(Identifier sequenceName)
    {
        using (await _lock.LockAsync().ConfigureAwait(false))
        {
            if (!_sequencesCache.TryGetValue(sequenceName, out var lazySequence))
            {
                lazySequence = new AsyncLazy<IDatabaseSequence>(() => SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync());
                _sequencesCache[sequenceName] = lazySequence;
            }

            return await lazySequence.ConfigureAwait(false);
        }
    }

    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseSequence>> _sequencesCache = [];

    [Test]
    public async Task GetSequence_WhenSequencePresent_ReturnsSequence()
    {
        var sequenceIsSome = await SequenceProvider.GetSequence("db_test_sequence_1").IsSome.ConfigureAwait(false);
        Assert.That(sequenceIsSome, Is.True);
    }

    [Test]
    public async Task GetSequence_WhenSequencePresent_ReturnsSequenceWithCorrectName()
    {
        const string sequenceName = "db_test_sequence_1";
        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name.LocalName, Is.EqualTo(sequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(sequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequenceMissing_ReturnsNone()
    {
        var sequenceIsNone = await SequenceProvider.GetSequence("sequence_that_doesnt_exist").IsNone.ConfigureAwait(false);
        Assert.That(sequenceIsNone, Is.True);
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("DB_TEST_sequence_1");
        var sequence = await SequenceProvider.GetSequence(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, sequence.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
    {
        var inputName = new Identifier("Dbo", "DB_TEST_sequence_1");
        var sequence = await SequenceProvider.GetSequence(inputName).UnwrapSomeAsync().ConfigureAwait(false);

        var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, sequence.Name.Schema)
            && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, sequence.Name.LocalName);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task GetAllSequences_WhenEnumerated_ContainsSequences()
    {
        var hasSequences = await SequenceProvider.GetAllSequences()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasSequences, Is.True);
    }

    [Test]
    public async Task GetAllSequences_WhenEnumerated_ContainsTestSequence()
    {
        var containsTestSequence = await SequenceProvider.GetAllSequences()
            .AnyAsync(s => string.Equals(s.Name.LocalName, "db_test_sequence_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestSequence, Is.True);
    }

    [Test]
    public async Task Start_GivenDefaultSequence_ReturnsLongMinValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

        Assert.That(sequence.Start, Is.EqualTo(long.MinValue));
    }

    [Test]
    public async Task Start_GivenSequenceWithCustomStart_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_2").ConfigureAwait(false);

        Assert.That(sequence.Start, Is.EqualTo(20));
    }

    [Test]
    public async Task Increment_GivenDefaultSequence_ReturnsOne()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

        Assert.That(sequence.Increment, Is.EqualTo(1));
    }

    [Test]
    public async Task Increment_GivenSequenceWithCustomIncrement_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_3").ConfigureAwait(false);

        Assert.That(sequence.Increment, Is.EqualTo(100));
    }

    [Test]
    public async Task MinValue_GivenDefaultSequence_ReturnsLongMinValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(long.MinValue));
    }

    [Test]
    public async Task MinValue_GivenSequenceWithCustomMinValue_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_4").ConfigureAwait(false);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(-99));
    }

    [Test]
    public async Task MinValue_GivenSequenceWithNoMinValue_ReturnsLongMinValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_5").ConfigureAwait(false);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(long.MinValue));
    }

    [Test]
    public async Task MaxValue_GivenDefaultSequence_ReturnsLongMaxValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(long.MaxValue));
    }

    [Test]
    public async Task MaxValue_GivenSequenceWithCustomMaxValue_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_6").ConfigureAwait(false);

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(333));
    }

    [Test]
    public async Task MaxValue_GivenSequenceWithNoMaxValue_ReturnsLongMaxValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_7").ConfigureAwait(false);

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(long.MaxValue));
    }

    [Test]
    public async Task Cycle_GivenDefaultSequence_ReturnsTrue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

        Assert.That(sequence.Cycle, Is.False);
    }

    [Test]
    public async Task Cycle_GivenSequenceWithCycle_ReturnsTrue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_8").ConfigureAwait(false);

        Assert.That(sequence.Cycle, Is.True);
    }

    [Test]
    public async Task Cycle_GivenSequenceWithNoCycle_ReturnsTrue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_9").ConfigureAwait(false);

        Assert.That(sequence.Cycle, Is.False);
    }

    [Test]
    public async Task Cache_GivenDefaultSequence_ReturnsNegativeOne()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_1").ConfigureAwait(false);

        Assert.That(sequence.Cache, Is.EqualTo(-1));
    }

    [Test]
    public async Task Cache_GivenSequenceWithCacheSet_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_10").ConfigureAwait(false);

        Assert.That(sequence.Cache, Is.EqualTo(10));
    }

    [Test]
    public async Task Cache_GivenSequenceWithNoCacheSet_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("db_test_sequence_11").ConfigureAwait(false);

        Assert.That(sequence.Cache, Is.EqualTo(0));
    }
}