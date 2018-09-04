using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests.Integration
{
    internal partial class SqlServerRelationalDatabaseTableTests : SqlServerTest
    {
        private IRelationalDatabase Database => new SqlServerRelationalDatabase(Dialect, Connection);

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
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
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
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint uk_test_table_7 unique (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_8 (
    test_column int,
    index ix_test_table_8 (test_column)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_9 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    index ix_test_table_9 (first_name, last_name, middle_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_10 (
    test_column int,
    test_column_2 int
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_10 on table_test_table_10 (test_column) include (test_column_2)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_11 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_11 on table_test_table_11 (first_name) include (last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_12 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_12 on table_test_table_12 (first_name) include (last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter index ix_test_table_12 on table_test_table_12 disable").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_13 (
    first_name nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ix_test_table_13 on table_test_table_13 (first_name, last_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_14 (
    test_column int not null,
    constraint ck_test_table_14 check ([test_column]>(1))
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_15 (
    first_name_parent nvarchar(50),
    middle_name_parent nvarchar(50),
    last_name_parent nvarchar(50),
    constraint pk_test_table_15 primary key (first_name_parent),
    constraint uk_test_table_15 unique (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_16 (
    first_name_child nvarchar(50),
    middle_name nvarchar(50),
    last_name nvarchar(50),
    constraint fk_test_table_16 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_17 (
    first_name nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_17 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_18 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_18 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_19 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_19 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_20 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_20 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_21 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_21 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_22 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_22 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_23 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_23 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on update set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_24 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_24 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_25 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_25 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_26 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_26 foreign key (first_name_child) references table_test_table_15 (first_name_parent) on delete set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_27 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_27 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_28 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_28 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_29 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_29 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent) on delete set default
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_30 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_30 foreign key (first_name_child) references table_test_table_15 (first_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter table table_test_table_30 nocheck constraint fk_test_table_30").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_31 (
    first_name_child nvarchar(50),
    middle_name_child nvarchar(50),
    last_name_child nvarchar(50),
    constraint fk_test_table_31 foreign key (last_name_child, middle_name_child) references table_test_table_15 (last_name_parent, middle_name_parent)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter table table_test_table_31 nocheck constraint fk_test_table_31").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table table_test_table_32 (
    test_column int not null,
    constraint ck_test_table_32 check ([test_column]>(1))
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("alter table table_test_table_32 nocheck constraint ck_test_table_32").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_33 ( test_column int not null default 1 )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create table table_test_table_34 (
    test_column_1 int,
    test_column_2 int,
    test_column_3 as test_column_1 + test_column_2
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table table_test_table_35 ( test_column int identity (10, 5) primary key )").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table trigger_test_table_1 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table trigger_test_table_2 (table_id int primary key not null)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_1
on trigger_test_table_1
for insert
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_2
on trigger_test_table_1
for update
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_3
on trigger_test_table_1
for delete
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_4
on trigger_test_table_1
after insert
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_5
on trigger_test_table_1
after update
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_6
on trigger_test_table_1
after delete
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_7
on trigger_test_table_1
instead of insert
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_8
on trigger_test_table_1
instead of update
as
begin
    declare @test int
end
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create trigger trigger_test_table_1_trigger_9
on trigger_test_table_1
instead of delete
as
begin
    declare @test int
end
").ConfigureAwait(false);
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
            await Connection.ExecuteAsync("drop table table_test_table_34").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table table_test_table_35").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table trigger_test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table trigger_test_table_2").ConfigureAwait(false);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Database;
            var tableName = new Identifier("table_test_table_1");
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