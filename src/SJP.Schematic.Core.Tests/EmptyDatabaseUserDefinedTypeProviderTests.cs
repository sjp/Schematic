using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyDatabaseUserDefinedTypeProviderTests
{
    [Test]
    public static void GetUserDefinedType_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseUserDefinedTypeProvider();
        Assert.That(() => provider.GetUserDefinedType(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetUserDefinedType_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseUserDefinedTypeProvider();
        var userDefinedType = provider.GetUserDefinedType("type_name");
        var typeIsNone = await userDefinedType.IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllUserDefinedTypes_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseUserDefinedTypeProvider();
        var hasTypes = await provider.EnumerateAllUserDefinedTypes().AnyAsync();

        Assert.That(hasTypes, Is.False);
    }

    [Test]
    public static async Task GetAllUserDefinedTypes_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseUserDefinedTypeProvider();
        var types = await provider.GetAllUserDefinedTypes();

        Assert.That(types, Is.Empty);
    }
}
