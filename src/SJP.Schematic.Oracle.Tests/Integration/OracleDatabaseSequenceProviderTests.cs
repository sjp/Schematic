using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabaseSequenceProviderTests : OracleTest
    {
        public OracleDatabaseSequenceProviderTests()
        {
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            var database = new OracleRelationalDatabase(Dialect, Connection, identifierResolver);
            IdentifierDefaults = new DatabaseIdentifierDefaultsBuilder()
                .WithServer(database.ServerName)
                .WithDatabase(database.DatabaseName)
                .WithSchema(database.DefaultSchema)
                .Build();
            SequenceProvider = new OracleDatabaseSequenceProvider(Connection, IdentifierDefaults, identifierResolver);
        }

        private IDatabaseIdentifierDefaults IdentifierDefaults { get; }
        private IDatabaseSequenceProvider SequenceProvider { get; }

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create sequence db_test_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_2 start with 20").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_3 start with 100 increment by 100").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_4 start with 1000 minvalue -99").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_5 start with 1000 nominvalue").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_6 start with 1 maxvalue 333").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_7 start with 1 nomaxvalue").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_8 cycle maxvalue 1000").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_9 nocycle").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_10 cache 10").ConfigureAwait(false);
            await Connection.ExecuteAsync("create sequence db_test_sequence_11 nocache").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop sequence db_test_sequence_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_9").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop sequence db_test_sequence_11").ConfigureAwait(false);
        }

        private const int SequenceDefaultCache = 20;
        private const int SequenceDefaultMinValue = 1;
        private const decimal OracleNumberMaxValue = 9999999999999999999999999999m;

        [Test]
        public void GetSequence_WhenSequencePresent_ReturnsSequence()
        {
            var sequence = SequenceProvider.GetSequence("db_test_sequence_1");
            Assert.IsTrue(sequence.IsSome);
        }

        [Test]
        public void GetSequence_WhenSequencePresent_ReturnsSequenceWithCorrectName()
        {
            const string sequenceName = "db_test_sequence_1";
            const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

            var sequence = SequenceProvider.GetSequence(sequenceName).UnwrapSome();

            Assert.AreEqual(expectedSequenceName, sequence.Name.LocalName);
        }

        [Test]
        public void GetSequence_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("DB_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = SequenceProvider.GetSequence(sequenceName).UnwrapSome();

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void GetSequence_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = SequenceProvider.GetSequence(sequenceName).UnwrapSome();

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void GetSequence_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = SequenceProvider.GetSequence(sequenceName).UnwrapSome();

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void GetSequence_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = SequenceProvider.GetSequence(sequenceName).UnwrapSome();

            Assert.AreEqual(sequenceName, sequence.Name);
        }

        [Test]
        public void GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequenceOption = SequenceProvider.GetSequence(sequenceName);
            var sequence = sequenceOption.UnwrapSome();

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void GetSequence_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequenceOption = SequenceProvider.GetSequence(sequenceName);
            var sequence = sequenceOption.UnwrapSome();

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void GetSequence_WhenSequenceMissing_ReturnsNone()
        {
            var sequence = SequenceProvider.GetSequence("sequence_that_doesnt_exist");
            Assert.IsTrue(sequence.IsNone);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresent_ReturnsSequence()
        {
            var sequenceIsSome = await SequenceProvider.GetSequenceAsync("db_test_sequence_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(sequenceIsSome);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresent_ReturnsSequenceWithCorrectName()
        {
            const string sequenceName = "db_test_sequence_1";
            const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name.LocalName);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("DB_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(sequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequencePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var sequenceName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_sequence_1");
            var expectedSequenceName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_SEQUENCE_1");

            var sequence = await SequenceProvider.GetSequenceAsync(sequenceName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public async Task GetSequenceAsync_WhenSequenceMissing_ReturnsNone()
        {
            var sequenceIsNone = await SequenceProvider.GetSequenceAsync("sequence_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(sequenceIsNone);
        }

        [Test]
        public void Sequences_WhenEnumerated_ContainsSequences()
        {
            var sequences = SequenceProvider.Sequences;

            Assert.NotZero(sequences.Count);
        }

        [Test]
        public void Sequences_WhenEnumerated_ContainsTestSequence()
        {
            const string expectedSequenceName = "DB_TEST_SEQUENCE_1";
            var containsTestSequence = SequenceProvider.Sequences.Any(s => s.Name.LocalName == expectedSequenceName);

            Assert.True(containsTestSequence);
        }

        [Test]
        public async Task SequencesAsync_WhenEnumerated_ContainsSequences()
        {
            var sequences = await SequenceProvider.SequencesAsync().ConfigureAwait(false);

            Assert.NotZero(sequences.Count);
        }

        [Test]
        public async Task SequencesAsync_WhenEnumerated_ContainsTestSequence()
        {
            const string expectedSequenceName = "DB_TEST_SEQUENCE_1";

            var sequences = await SequenceProvider.SequencesAsync().ConfigureAwait(false);
            var containsTestSequence = sequences.Any(s => s.Name.LocalName == expectedSequenceName);

            Assert.True(containsTestSequence);
        }

        [Test]
        public void Start_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_1").UnwrapSome();

            Assert.AreEqual(1, sequence.Start);
        }

        [Test]
        public void Start_GivenSequenceWithCustomStart_ReturnsCorrectValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_2").UnwrapSome();

            Assert.AreEqual(1, sequence.Start);
        }

        [Test]
        public void Increment_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_1").UnwrapSome();

            Assert.AreEqual(1, sequence.Increment);
        }

        [Test]
        public void Increment_GivenSequenceWithCustomIncrement_ReturnsCorrectValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_3").UnwrapSome();

            Assert.AreEqual(100, sequence.Increment);
        }

        [Test]
        public void MinValue_GivenDefaultSequence_ReturnsOne()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_1").UnwrapSome();

            Assert.AreEqual(SequenceDefaultMinValue, sequence.MinValue);
        }

        [Test]
        public void MinValue_GivenSequenceWithCustomMinValue_ReturnsCorrectValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_4").UnwrapSome();

            Assert.AreEqual(-99, sequence.MinValue);
        }

        [Test]
        public void MinValue_GivenAscendingSequenceWithNoMinValue_ReturnsOne()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_5").UnwrapSome();

            Assert.AreEqual(1, sequence.MinValue);
        }

        [Test]
        public void MaxValue_GivenDefaultSequence_ReturnsOracleNumberMaxValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_1").UnwrapSome();

            Assert.AreEqual(OracleNumberMaxValue, sequence.MaxValue);
        }

        [Test]
        public void MaxValue_GivenSequenceWithCustomMaxValue_ReturnsCorrectValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_6").UnwrapSome();

            Assert.AreEqual(333, sequence.MaxValue);
        }

        [Test]
        public void MaxValue_GivenSequenceWithNoMaxValue_ReturnsOracleNumberMaxValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_7").UnwrapSome();

            Assert.AreEqual(OracleNumberMaxValue, sequence.MaxValue);
        }

        [Test]
        public void Cycle_GivenDefaultSequence_ReturnsTrue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_1").UnwrapSome();

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public void Cycle_GivenSequenceWithCycle_ReturnsTrue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_8").UnwrapSome();

            Assert.IsTrue(sequence.Cycle);
        }

        [Test]
        public void Cycle_GivenSequenceWithNoCycle_ReturnsTrue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_9").UnwrapSome();

            Assert.IsFalse(sequence.Cycle);
        }

        [Test]
        public void Cache_GivenDefaultSequence_ReturnsDefaultCacheSize()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_1").UnwrapSome();

            Assert.AreEqual(SequenceDefaultCache, sequence.Cache);
        }

        [Test]
        public void Cache_GivenSequenceWithCacheSet_ReturnsCorrectValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_10").UnwrapSome();

            Assert.AreEqual(10, sequence.Cache);
        }

        [Test]
        public void Cache_GivenSequenceWithNoCacheSet_ReturnsCorrectValue()
        {
            var sequence = SequenceProvider.GetSequence("DB_TEST_SEQUENCE_11").UnwrapSome();

            Assert.AreEqual(0, sequence.Cache);
        }
    }
}
