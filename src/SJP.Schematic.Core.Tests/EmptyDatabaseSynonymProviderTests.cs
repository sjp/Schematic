using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyDatabaseSynonymProviderTests
{
    [Test]
    public static void GetSynonym_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseSynonymProvider();
        Assert.That(() => provider.GetSynonym(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSynonym_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseSynonymProvider();
        var synonym = provider.GetSynonym("synonym_name");
        var synonymIsNone = await synonym.IsNone;

        Assert.That(synonymIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSynonyms_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSynonymProvider();
        var hasSynonyms = await provider.EnumerateAllSynonyms().AnyAsync();

        Assert.That(hasSynonyms, Is.False);
    }

    [Test]
    public static async Task GetAllSynonyms_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSynonymProvider();
        var synonyms = await provider.GetAllSynonyms();

        Assert.That(synonyms, Is.Empty);
    }
}