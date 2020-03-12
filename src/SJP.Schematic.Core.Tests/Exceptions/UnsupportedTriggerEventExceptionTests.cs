using NUnit.Framework;
using SJP.Schematic.Core.Exceptions;

namespace SJP.Schematic.Core.Tests.Exceptions
{
    [TestFixture]
    internal static class UnsupportedTriggerEventExceptionTests
    {
        [TestCase("testTableName", "UNKNOWN_EVENT", "Found an unsupported trigger event name for a trigger on the table 'LocalName = testTableName'. Expected one of INSERT, UPDATE, DELETE, got: UNKNOWN_EVENT")]
        [TestCase("tableName", "UNKNOWN_EVENT", "Found an unsupported trigger event name for a trigger on the table 'LocalName = tableName'. Expected one of INSERT, UPDATE, DELETE, got: UNKNOWN_EVENT")]
        public static void Message_PropertyGet_ReturnsExpectedValue(string tableName, string triggerEvent, string expectedOutput)
        {
            var name = Identifier.CreateQualifiedIdentifier(tableName);
            var ex = new UnsupportedTriggerEventException(name, triggerEvent);

            Assert.That(ex.Message, Is.EqualTo(expectedOutput));
        }
    }
}
