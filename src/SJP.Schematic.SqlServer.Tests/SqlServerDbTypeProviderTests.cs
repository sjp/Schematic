using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests;

[TestFixture]
internal static class SqlServerDbTypeProviderTests
{
    // TODO
    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqlServerDbTypeProvider(), Throws.Nothing);
    }
}