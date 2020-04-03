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

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
        {
            Identifier routineName = "test_routine";

            Assert.That(() => new DatabaseRoutine(routineName, definition), Throws.ArgumentNullException);
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

        [TestCase("", "test_routine", "Routine: test_routine")]
        [TestCase("test_schema", "test_routine", "Routine: test_schema.test_routine")]
        public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
        {
            var routineName = Identifier.CreateQualifiedIdentifier(schema, localName);
            const string definition = "create function test_function...";
            var routine = new DatabaseRoutine(routineName, definition);

            var result = routine.ToString();

            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
