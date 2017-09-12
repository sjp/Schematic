using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    [TestFixture]
    internal class SqlServerRelationalDatabaseTableTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Task.Delay(1);
            //await Connection.ExecuteAsync("create view view_test_view_1 as select 1 as test").ConfigureAwait(false);
            //await Connection.ExecuteAsync("create table view_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            //await Connection.ExecuteAsync("create view view_test_view_2 with schemabinding as select table_id as test from [dbo].[view_test_table_1]").ConfigureAwait(false);
            //await Connection.ExecuteAsync("create unique clustered index ix_view_test_view_2 on view_test_view_2 (test)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Task.Delay(1);
            //await Connection.ExecuteAsync("drop view view_test_view_1").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop view view_test_view_2").ConfigureAwait(false);
            //await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseTable(null, Database, "test"));
        }

        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseTable(Connection, null, "test"));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalDatabaseTable(Connection, Database, null));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Database;
            var table = new SqlServerRelationalDatabaseTable(Connection, database, "table_test_table_1");

            Assert.AreSame(database, table.Database);
        }

        [Test]
        public void Name_PropertyGet_ShouldEqualCtorArg()
        {
            const string tableName = "table_test_table_1";
            var table = new SqlServerRelationalDatabaseTable(Connection, Database, tableName);

            Assert.AreEqual(tableName, table.Name.LocalName);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new LocalIdentifier("table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("asd", "table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, "asd", "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("qwe", "asd", "table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, "qwe", "asd", "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");
            var expectedTableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");

            var table = new SqlServerRelationalDatabaseTable(Connection, Database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }
    }
}
