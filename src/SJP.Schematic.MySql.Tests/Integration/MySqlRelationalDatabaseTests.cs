using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal sealed class MySqlRelationalDatabaseTests : MySqlTest
    {
        private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual(Connection.Database, Database.DefaultSchema);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonNull()
        {
            Assert.IsNotNull(Database.DatabaseVersion);
        }

        [Test]
        public void DatabaseVersion_PropertyGet_ShouldBeNonEmpty()
        {
            Assert.AreNotEqual(string.Empty, Database.DatabaseVersion);
        }

        internal sealed class SequenceTests : MySqlTest
        {
            private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequence(null));
            }

            [Test]
            public void GetSequence_GivenValidSequenceName_ReturnsNone()
            {
                var sequenceName = new Identifier("asd");
                var sequence = Database.GetSequence(sequenceName);

                Assert.IsTrue(sequence.IsNone);
            }

            [Test]
            public void Sequences_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = Database.Sequences.ToList();
                var count = sequences.Count;

                Assert.Zero(count);
            }

            [Test]
            public void GetSequenceAsync_GivenNullSequenceName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSequenceAsync(null));
            }

            [Test]
            public async Task GetSequenceAsync_GivenValidSequenceName_ReturnsNone()
            {
                var sequenceName = new Identifier("asd");
                var sequenceIsNone = await Database.GetSequenceAsync(sequenceName).IsNone.ConfigureAwait(false);

                Assert.IsTrue(sequenceIsNone);
            }

            [Test]
            public async Task SequencesAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var sequences = await Database.SequencesAsync().ConfigureAwait(false);

                Assert.Zero(sequences.Count);
            }
        }

        internal sealed class SynonymTests : MySqlTest
        {
            private IRelationalDatabase Database => new MySqlRelationalDatabase(Dialect, Connection);

            [Test]
            public void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_GivenValidSynonymName_ReturnsNone()
            {
                var synonymName = new Identifier("asd");
                var synonym = Database.GetSynonym(synonymName);

                Assert.IsTrue(synonym.IsNone);
            }

            [Test]
            public void Synonyms_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = Database.Synonyms.ToList();
                var count = synonyms.Count;

                Assert.Zero(count);
            }

            [Test]
            public void GetSynonymAsync_GivenNullSynonymName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public async Task GetSynonymAsync_GivenValidSynonymName_ReturnsNone()
            {
                var synonymName = new Identifier("asd");
                var synonymIsNone = await Database.GetSynonymAsync(synonymName).IsNone.ConfigureAwait(false);

                Assert.IsTrue(synonymIsNone);
            }

            [Test]
            public async Task SynonymsAsync_PropertyGet_ReturnsEmptyCollection()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

                Assert.Zero(synonyms.Count);
            }
        }
    }
}
