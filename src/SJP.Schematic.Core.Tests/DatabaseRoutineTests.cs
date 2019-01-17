using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseRoutineTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string definition = "create function test_function...";

            Assert.Throws<ArgumentNullException>(() => new DatabaseRoutine(null, definition));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier routineName = "test_routine";

            Assert.Throws<ArgumentNullException>(() => new DatabaseRoutine(routineName, null));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier routineName = "test_routine";

            Assert.Throws<ArgumentNullException>(() => new DatabaseRoutine(routineName, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier routineName = "test_routine";

            Assert.Throws<ArgumentNullException>(() => new DatabaseRoutine(routineName, "    "));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier routineName = "test_routine";
            const string definition = "create function test_function...";

            var routine = new DatabaseRoutine(routineName, definition);

            Assert.AreEqual(routineName, routine.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier routineName = "test_routine";
            const string definition = "create function test_function...";

            var routine = new DatabaseRoutine(routineName, definition);

            Assert.AreEqual(definition, routine.Definition);
        }
    }
}
