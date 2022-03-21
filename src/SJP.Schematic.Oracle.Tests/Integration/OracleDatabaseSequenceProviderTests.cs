﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed class OracleDatabaseSequenceProviderTests : OracleTest
{
    private IDatabaseSequenceProvider SequenceProvider => new OracleDatabaseSequenceProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_2 start with 20", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_3 start with 100 increment by 100", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_4 start with 1000 minvalue -99", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_5 start with 1000 nominvalue", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_6 start with 1 maxvalue 333", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_7 start with 1 nomaxvalue", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_8 cycle maxvalue 1000", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_9 nocycle", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_10 cache 10", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create sequence db_test_sequence_11 nocache", CancellationToken.None).ConfigureAwait(false);
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
        if (sequenceName == null)
            throw new ArgumentNullException(nameof(sequenceName));

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
    private readonly Dictionary<Identifier, AsyncLazy<IDatabaseSequence>> _sequencesCache = new();

    private const int SequenceDefaultCache = 20;
    private const int SequenceDefaultMinValue = 1;
    private const decimal OracleNumberMaxValue = 9999999999999999999999999999m;

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
        const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name.LocalName, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("DB_TEST_SEQUENCE_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(sequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

        var sequence = await SequenceProvider.GetSequence(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(sequence.Name, Is.EqualTo(expectedSequenceName));
    }

    [Test]
    public async Task GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_sequence_1");
        var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

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
        const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

        var containsTestSequence = await SequenceProvider.GetAllSequences()
            .AnyAsync(s => string.Equals(s.Name.LocalName, expectedSequenceName, StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestSequence, Is.True);
    }

    [Test]
    public async Task Start_GivenDefaultSequence_ReturnsOne()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_1").ConfigureAwait(false);

        Assert.That(sequence.Start, Is.EqualTo(1));
    }

    [Test]
    public async Task Start_GivenSequenceWithCustomStart_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_2").ConfigureAwait(false);

        Assert.That(sequence.Start, Is.EqualTo(1));
    }

    [Test]
    public async Task Increment_GivenDefaultSequence_ReturnsOne()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_1").ConfigureAwait(false);

        Assert.That(sequence.Increment, Is.EqualTo(1));
    }

    [Test]
    public async Task Increment_GivenSequenceWithCustomIncrement_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_3").ConfigureAwait(false);

        Assert.That(sequence.Increment, Is.EqualTo(100));
    }

    [Test]
    public async Task MinValue_GivenDefaultSequence_ReturnsOne()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_1").ConfigureAwait(false);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(SequenceDefaultMinValue));
    }

    [Test]
    public async Task MinValue_GivenSequenceWithCustomMinValue_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_4").ConfigureAwait(false);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(-99));
    }

    [Test]
    public async Task MinValue_GivenAscendingSequenceWithNoMinValue_ReturnsOne()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_5").ConfigureAwait(false);

        Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(1));
    }

    [Test]
    public async Task MaxValue_GivenDefaultSequence_ReturnsOracleNumberMaxValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_1").ConfigureAwait(false);

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(OracleNumberMaxValue));
    }

    [Test]
    public async Task MaxValue_GivenSequenceWithCustomMaxValue_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_6").ConfigureAwait(false);

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(333));
    }

    [Test]
    public async Task MaxValue_GivenSequenceWithNoMaxValue_ReturnsOracleNumberMaxValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_7").ConfigureAwait(false);

        Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(OracleNumberMaxValue));
    }

    [Test]
    public async Task Cycle_GivenDefaultSequence_ReturnsTrue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_1").ConfigureAwait(false);

        Assert.That(sequence.Cycle, Is.False);
    }

    [Test]
    public async Task Cycle_GivenSequenceWithCycle_ReturnsTrue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_8").ConfigureAwait(false);

        Assert.That(sequence.Cycle, Is.True);
    }

    [Test]
    public async Task Cycle_GivenSequenceWithNoCycle_ReturnsTrue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_9").ConfigureAwait(false);

        Assert.That(sequence.Cycle, Is.False);
    }

    [Test]
    public async Task Cache_GivenDefaultSequence_ReturnsDefaultCacheSize()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_1").ConfigureAwait(false);

        Assert.That(sequence.Cache, Is.EqualTo(SequenceDefaultCache));
    }

    [Test]
    public async Task Cache_GivenSequenceWithCacheSet_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_10").ConfigureAwait(false);

        Assert.That(sequence.Cache, Is.EqualTo(10));
    }

    [Test]
    public async Task Cache_GivenSequenceWithNoCacheSet_ReturnsCorrectValue()
    {
        var sequence = await GetSequenceAsync("DB_TEST_SEQUENCE_11").ConfigureAwait(false);

        Assert.That(sequence.Cache, Is.Zero);
    }
}