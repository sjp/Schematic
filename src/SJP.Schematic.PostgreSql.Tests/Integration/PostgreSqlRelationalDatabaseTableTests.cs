using System;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests.Integration
{
    [TestFixture]
    internal partial class PostgreSqlRelationalDatabaseTableTests : PostgreSqlTest
    {
        private IRelationalDatabase Database => new PostgreSqlRelationalDatabase(Dialect, Connection);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table table_test_table_1 ( test_column int )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_2 ( test_column int not null primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_3 (
    test_column int,
    constraint pk_test_table_3 primary key (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_4 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint pk_test_table_4 primary key (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_5 ( test_column int not null unique )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_6 (
    test_column int,
    constraint uk_test_table_6 unique (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_7 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_8 (
    test_column int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_8 on table_test_table_8 (test_column)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_9 on table_test_table_9 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_10 (
    test_column int,
    test_column_2 int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_10 on table_test_table_10 (test_column)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_11 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_11 on table_test_table_11 (first_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_12 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_12 on table_test_table_12 (first_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name varchar(50),
    middle_name varchar(50),
    last_name varchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check (test_column > 1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent varchar(50),
    middle_name_parent varchar(50),
    last_name_parent varchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child varchar(50),
    middle_name varchar(50),
    last_name varchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_18 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_19 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_20 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_21 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_22 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_23 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_26 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_29 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child varchar(50),
    middle_name_child varchar(50),
    last_name_child varchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check (test_column > 1)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_33 ( test_column int not null default 1 )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_35 ( test_column serial primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table trigger_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table trigger_test_table_2 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create function test_trigger_fn()
returns trigger as
$BODY$
BEGIN
    RETURN null;
END;
$BODY$
LANGUAGE PLPGSQL").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_1
before insert
on trigger_test_table_1
execute procedure test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_2
before update
on trigger_test_table_1
execute procedure test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_3
before delete
on trigger_test_table_1
execute procedure test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_4
after insert
on trigger_test_table_1
execute procedure test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_5
after update
on trigger_test_table_1
execute procedure test_trigger_fn()").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_6
after delete
on trigger_test_table_1
execute procedure test_trigger_fn()").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table table_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_5").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_6").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_7").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_8").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_9").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_10").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_11").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_12").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_13").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_14").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_16").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_17").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_18").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_19").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_20").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_21").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_22").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_23").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_24").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_25").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_26").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_27").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_28").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_29").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_30").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_31").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_15").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_32").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_33").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_35").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table trigger_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table trigger_test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop function test_trigger_fn").ConfigureAwait(false);
        }

        [Test]
        public void Ctor_GivenNullConnection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(null, Database, "test"));
        }

        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(Connection, null, "test"));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlRelationalDatabaseTable(Connection, Database, null));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Database;
            var table = new PostgreSqlRelationalDatabaseTable(Connection, database, "table_test_table_1");

            Assert.AreSame(database, table.Database);
        }

        [Test]
        public void Name_PropertyGet_ShouldEqualCtorArg()
        {
            const string tableName = "table_test_table_1";
            var table = new PostgreSqlRelationalDatabaseTable(Connection, Database, tableName);

            Assert.AreEqual(tableName, table.Name.LocalName);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "table_test_table_1");

            var table = new PostgreSqlRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("asd", "table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, database.DatabaseName, "asd", "table_test_table_1");

            var table = new PostgreSqlRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("qwe", "asd", "table_test_table_1");
            var expectedTableName = new Identifier(database.ServerName, "qwe", "asd", "table_test_table_1");

            var table = new PostgreSqlRelationalDatabaseTable(Connection, database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");
            var expectedTableName = new Identifier("qwe", "asd", "zxc", "table_test_table_1");

            var table = new PostgreSqlRelationalDatabaseTable(Connection, Database, tableName);

            Assert.AreEqual(expectedTableName, table.Name);
        }
    }
}