using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyDatabaseRoutineProviderTests
{
    [Test]
    public static void GetRoutine_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseRoutineProvider();
        Assert.That(() => provider.GetRoutine(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetRoutine_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseRoutineProvider();
        var routine = provider.GetRoutine("routine_name");
        var routineIsNone = await routine.IsNone.ConfigureAwait(false);

        Assert.That(routineIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllRoutines_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseRoutineProvider();
        var hasRoutines = await provider.EnumerateAllRoutines()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasRoutines, Is.False);
    }

    [Test]
    public static async Task GetAllRoutines_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseRoutineProvider();
        var routines = await provider.GetAllRoutines().ConfigureAwait(false);

        Assert.That(routines, Is.Empty);
    }
}