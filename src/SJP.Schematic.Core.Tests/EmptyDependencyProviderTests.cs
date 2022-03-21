using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyDependencyProviderTests
{
    [Test]
    public static void GetDependencies_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDependencyProvider();
        Assert.That(() => provider.GetDependencies(null, "select * from test"), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetDependencies_GivenValidName_HasNoValues()
    {
        var provider = new EmptyDependencyProvider();
        var dependencies = provider.GetDependencies("test_table", "select * from test_table");

        Assert.That(dependencies, Is.Empty);
    }
}