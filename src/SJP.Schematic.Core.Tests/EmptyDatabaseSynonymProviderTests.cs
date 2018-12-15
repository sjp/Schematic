using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyDatabaseSynonymProviderTests
    {
        [Test]
        public static void GetSynonym_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSynonym(null));
        }

        [Test]
        public static async Task GetSynonym_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonym = provider.GetSynonym("synonym_name");
            var synonymIsNone = await synonym.IsNone.ConfigureAwait(false);

            Assert.IsTrue(synonymIsNone);
        }

        [Test]
        public static async Task GetAllSynonyms_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonyms = await provider.GetAllSynonyms().ConfigureAwait(false);

            Assert.Zero(synonyms.Count);
        }

        [Test]
        public static async Task GetAllSynonyms_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseSynonymProvider();
            var synonyms = await provider.GetAllSynonyms().ConfigureAwait(false);
            var synonymsList = synonyms.ToList();

            Assert.Zero(synonymsList.Count);
        }
    }
}
