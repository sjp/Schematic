using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    internal sealed class PostgreSqlRelationalDatabaseTests : PostgreSqlTest
    {
        private IRelationalDatabase Database => new PostgreSqlRelationalDatabase(Dialect, Connection, new DefaultPostgreSqlIdentifierResolutionStrategy());

        [Test]
        public void Database_PropertyGet_ShouldMatchConnectionDatabase()
        {
            Assert.AreEqual(Database.DatabaseName, Connection.Database);
        }

        [Test]
        public void DefaultSchema_PropertyGet_ShouldEqualConnectionDefaultSchema()
        {
            Assert.AreEqual("public", Database.DefaultSchema);
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

        internal sealed class SynonymTests : PostgreSqlTest
        {
            private IRelationalDatabase Database => new PostgreSqlRelationalDatabase(Dialect, Connection, new DefaultPostgreSqlIdentifierResolutionStrategy());

            [Test]
            public void GetSynonym_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonym(null));
            }

            [Test]
            public void GetSynonym_WhenSynonymMissing_ReturnsNone()
            {
                var synonym = Database.GetSynonym("synonym_that_doesnt_exist");
                Assert.IsTrue(synonym.IsNone);
            }

            [Test]
            public void GetSynonymAsync_GivenNullName_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => Database.GetSynonymAsync(null));
            }

            [Test]
            public async Task GetSynonymAsync_WhenSynonymMissing_ReturnsNone()
            {
                var synonymIsNone = await Database.GetSynonymAsync("synonym_that_doesnt_exist").IsNone.ConfigureAwait(false);
                Assert.IsTrue(synonymIsNone);
            }

            [Test]
            public void Synonyms_WhenEnumerated_ContainsNoSynonyms()
            {
                var synonyms = Database.Synonyms.ToList();

                Assert.Zero(synonyms.Count);
            }

            [Test]
            public async Task SynonymsAsync_WhenEnumerated_ContainsNoSynonyms()
            {
                var synonyms = await Database.SynonymsAsync().ConfigureAwait(false);

                Assert.Zero(synonyms.Count);
            }
        }
    }
}
