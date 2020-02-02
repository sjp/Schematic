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

            Assert.That(() => new DatabaseRoutine(null, definition), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier routineName = "test_routine";

            Assert.That(() => new DatabaseRoutine(routineName, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier routineName = "test_routine";

            Assert.That(() => new DatabaseRoutine(routineName, string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Identifier routineName = "test_routine";

            Assert.That(() => new DatabaseRoutine(routineName, "    "), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier routineName = "test_routine";
            const string definition = "create function test_function...";

            var routine = new DatabaseRoutine(routineName, definition);

            Assert.That(routine.Name, Is.EqualTo(routineName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier routineName = "test_routine";
            const string definition = "create function test_function...";

            var routine = new DatabaseRoutine(routineName, definition);

            Assert.That(routine.Definition, Is.EqualTo(definition));
        }
    }
}
