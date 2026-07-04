using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

// Uses a known relationship sub-graph in the Sakila sample database:
//   actor <- film_actor -> film
// i.e. "film_actor" is a child table of "actor" (via actor_id) and a parent-referencing table of
// "film" (via film_id). This gives predictable degree-0/1/2 traversal results to assert against.
internal sealed class RelationshipFinderSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        Assert.That(() => new RelationshipFinder(null!), Throws.ArgumentNullException);
    }

    [Test]
    public async Task GetTablesByDegrees_GivenNullTable_ThrowsArgumentNullException()
    {
        var database = GetDatabase();
        var actor = await database.GetTable("actor").UnwrapSomeAsync();
        var finder = new RelationshipFinder([actor]);

        Assert.That(() => finder.GetTablesByDegrees(null!, 1), Throws.ArgumentNullException);
    }

    [Test]
    public async Task GetTablesByDegrees_GivenZeroDegrees_ReturnsOnlySeedTable()
    {
        var database = GetDatabase();
        var (actor, filmActor, film) = await GetActorFilmActorFilmAsync(database);
        var finder = new RelationshipFinder([actor, filmActor, film]);

        var result = finder.GetTablesByDegrees(actor, 0);

        Assert.That(result.Select(static t => t.Name.LocalName), Is.EquivalentTo(new[] { "actor" }));
    }

    [Test]
    public async Task GetTablesByDegrees_GivenOneDegree_ReturnsSeedAndDirectlyRelatedTables()
    {
        var database = GetDatabase();
        var (actor, filmActor, film) = await GetActorFilmActorFilmAsync(database);
        var finder = new RelationshipFinder([actor, filmActor, film]);

        var result = finder.GetTablesByDegrees(actor, 1);

        Assert.That(result.Select(static t => t.Name.LocalName), Is.EquivalentTo(new[] { "actor", "film_actor" }));
    }

    [Test]
    public async Task GetTablesByDegrees_GivenTwoDegrees_ReturnsWidenedSetOfRelatedTables()
    {
        var database = GetDatabase();
        var (actor, filmActor, film) = await GetActorFilmActorFilmAsync(database);
        var finder = new RelationshipFinder([actor, filmActor, film]);

        var result = finder.GetTablesByDegrees(actor, 2);

        Assert.That(result.Select(static t => t.Name.LocalName), Is.EquivalentTo(new[] { "actor", "film_actor", "film" }));
    }

    [Test]
    public async Task GetTablesByDegrees_GivenRelatedTableNotInSuppliedSet_ExcludesItFromResult()
    {
        var database = GetDatabase();
        var actor = await database.GetTable("actor").UnwrapSomeAsync();

        // Only "actor" is known to the finder -- "film_actor" is related in the full schema, but
        // absent here, so it must never appear in the result.
        var finder = new RelationshipFinder([actor]);

        var result = finder.GetTablesByDegrees(actor, 5);

        Assert.That(result.Select(static t => t.Name.LocalName), Is.EquivalentTo(new[] { "actor" }));
    }

    private static async Task<(IRelationalDatabaseTable Actor, IRelationalDatabaseTable FilmActor, IRelationalDatabaseTable Film)> GetActorFilmActorFilmAsync(ISqliteDatabase database)
    {
        var actor = await database.GetTable("actor").UnwrapSomeAsync();
        var filmActor = await database.GetTable("film_actor").UnwrapSomeAsync();
        var film = await database.GetTable("film").UnwrapSomeAsync();

        return (actor, filmActor, film);
    }
}
