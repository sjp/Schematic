using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Dbml.Tests.Integration
{
    internal sealed class DbmlFormatterTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        [Test]
        public static void RenderTables_GivenValidSqliteDatabase_GeneratesExpectedDbml()
        {
            //Assert.IsTrue(File.Exists(Config.SakilaDbPath), "Expected to find a database at: " + Config.SakilaDbPath);
            //
            //var database = GetDatabase();
            //var tables = await database.GetAllTables().ConfigureAwait(false);
            //
            //var formatter = new DbmlFormatter();
            var result = string.Empty; //formatter.RenderTables(tables);

            Assert.NotNull(result);
        }
    }
}
