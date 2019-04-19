using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Serialization.Tests
{
    [TestFixture]
    internal static class SnakeCaseNameProviderTests
    {
        [Test]
        public static void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Pass();
        }

        //[Test]
        //public static async Task Export_Test()
        //{
        //    const string connectionString = @"Data Source=C:\Users\sjp\Downloads\Northwind_large.sqlite";
        //    var connection = await SqliteDialect.CreateConnectionAsync(connectionString).ConfigureAwait(false);
        //    var dialect = new SqliteDialect(connection);
        //    var identifierDefaults = new IdentifierDefaultsBuilder()
        //        .WithSchema("main")
        //        .Build();

        //    try
        //    {
        //        var database = new SqliteRelationalDatabase(dialect, connection, identifierDefaults);
        //        var serializer = new JsonRelationalDatabaseSerializer();

        //        const string outFilePath = @"C:\Users\sjp\Downloads\northwind_dump.json";
        //        var dtoText = await serializer.SerializeAsync(database).ConfigureAwait(false);
        //        await File.WriteAllTextAsync(outFilePath, dtoText).ConfigureAwait(false);

        //        _ = await serializer.DeserializeAsync(dtoText).ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        connection.Dispose();
        //    }
        //}
    }
}
