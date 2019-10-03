using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyDatabaseRoutineProviderTests
    {
        [Test]
        public static void GetRoutine_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseRoutineProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetRoutine(null));
        }

        [Test]
        public static async Task GetRoutine_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseRoutineProvider();
            var routine = provider.GetRoutine("routine_name");
            var routineIsNone = await routine.IsNone.ConfigureAwait(false);

            Assert.IsTrue(routineIsNone);
        }

        [Test]
        public static async Task GetAllRoutines_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseRoutineProvider();
            var hasRoutines = await provider.GetAllRoutines()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsFalse(hasRoutines);
        }
    }
}
