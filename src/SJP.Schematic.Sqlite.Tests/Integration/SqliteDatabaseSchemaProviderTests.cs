using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed class SqliteDatabaseSchemaProviderTests : SqliteTest
{
    private SqliteDatabaseSchemaProvider SchemaProvider => new(Pragma, IdentifierDefaults);

    [Test]
    public async Task GetAllSchemas_WhenInvoked_ReturnsTheMainDatabaseAsTheDefaultSchema()
    {
        var schemas = await SchemaProvider.GetAllSchemas();
        var main = schemas.Single(s => s.Name.LocalName == "main");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(main.IsDefault, Is.True);
            Assert.That(main.IsSystem, Is.False);
            Assert.That(main.Owner, Is.EqualTo(LanguageExt.Option<string>.None));
        }
    }

    [Test]
    public async Task EnumerateAllSchemas_WhenInvoked_MatchesGetAllSchemas()
    {
        var enumerated = await SchemaProvider.EnumerateAllSchemas().ToListAsync();
        var retrieved = await SchemaProvider.GetAllSchemas();

        Assert.That(
            enumerated.Select(s => s.Name.LocalName),
            Is.EqualTo(retrieved.Select(s => s.Name.LocalName)));
    }
}
