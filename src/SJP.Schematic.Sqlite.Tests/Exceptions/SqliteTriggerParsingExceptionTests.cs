using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Exceptions;

namespace SJP.Schematic.Sqlite.Tests.Exceptions
{
    [TestFixture]
    internal static class SqliteTriggerParsingExceptionTests
    {
        [TestCase("", "test_table", "Unable to parse the TRIGGER statement for the table 'LocalName = test_table'.")]
        [TestCase("test_schema", "test_table", "Unable to parse the TRIGGER statement for the table 'Schema = test_schema, LocalName = test_table'.")]
        public static void Message_PropertyGet_ConstructsExpectedMessage(string schema, string localName, string expectedOutput)
        {
            var tableName = Identifier.CreateQualifiedIdentifier(schema, localName);
            const string sql = "create trigger on ...";
            const string errorMessage = "unknown error";
            var ex = new SqliteTriggerParsingException(tableName, sql, errorMessage);

            Assert.That(ex.Message, Is.EqualTo(expectedOutput));
        }

        [Test]
        public static void TableName_PropertyGet_MatchesCtorArg()
        {
            var tableName = Identifier.CreateQualifiedIdentifier("test_table");
            const string sql = "create trigger on ...";
            const string errorMessage = "unknown error";
            var ex = new SqliteTriggerParsingException(tableName, sql, errorMessage);

            Assert.That(ex.TableName, Is.EqualTo(tableName));
        }

        [Test]
        public static void Sql_PropertyGet_MatchesCtorArg()
        {
            const string tableName = "test_table";
            const string sql = "create trigger on ...";
            const string errorMessage = "unknown error";
            var ex = new SqliteTriggerParsingException(tableName, sql, errorMessage);

            Assert.That(ex.Sql, Is.EqualTo(sql));
        }

        [Test]
        public static void ParsingErrorMessage_PropertyGet_MatchesCtorArg()
        {
            const string tableName = "test_table";
            const string sql = "create trigger on ...";
            const string errorMessage = "unknown error";
            var ex = new SqliteTriggerParsingException(tableName, sql, errorMessage);

            Assert.That(ex.ParsingErrorMessage, Is.EqualTo(errorMessage));
        }
    }
}
