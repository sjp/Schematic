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
        var synonymIsNone = await synonym.IsNone.ConfigureAwait(false);

        Assert.That(synonymIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllSynonyms_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSynonymProvider();
        var hasSynonyms = await provider.GetAllSynonyms().AnyAsync().ConfigureAwait(false);

        Assert.That(hasSynonyms, Is.False);
    }
}
