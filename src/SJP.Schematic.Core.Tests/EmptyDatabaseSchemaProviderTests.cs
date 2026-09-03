using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyDatabaseSchemaProviderTests
{
    [Test]
    public static async Task EnumerateAllSchemas_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSchemaProvider();
        var hasSchemas = await provider.EnumerateAllSchemas().AnyAsync();

        Assert.That(hasSchemas, Is.False);
    }

    [Test]
    public static async Task GetAllSchemas_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSchemaProvider();
        var schemas = await provider.GetAllSchemas();

        Assert.That(schemas, Is.Empty);
    }
}
