using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Dot.Tests.Integration
{
    internal sealed class DotFormatterTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private IDotFormatter Formatter => new DotFormatter(IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync(@"
create table test_table_1 (
    test_pk integer not null,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'test',
    constraint test_table_1_pk primary key (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_2 (
    test_pk_1 integer not null,
    test_pk_2 integer not null,
    first_name text not null,
    middle_name text not null,
    last_name text not null,
    comment text null,
    constraint test_table_2_pk primary key (test_pk_1, test_pk_2),
    constraint test_table_2_multi_uk unique (first_name, middle_name, last_name)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_2_first_name on test_table_2 (first_name, last_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create index ix_test_table_2_comment on test_table_2 (comment)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create unique index ux_test_table_2_first_name_middle_name on test_table_2 (first_name, middle_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create unique index ux_test_table_2_last_name on test_table_2 (last_name)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_3 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'test'
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_4 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'test',
    test_table_3_fk1 integer,
    test_table_3_fk2 integer,
    test_table_3_fk3 integer,
    test_table_3_fk4 integer,
    constraint fk_test_table_4_test_table_3_fk1 foreign key (test_table_3_fk1) references test_table_3 (test_pk),
    constraint fk_test_table_4_test_table_3_fk2 foreign key (test_table_3_fk2) references test_table_3 (test_pk) on update cascade,
    constraint fk_test_table_4_test_table_3_fk3 foreign key (test_table_3_fk3) references test_table_3 (test_pk) on delete set null,
    constraint fk_test_table_4_test_table_3_fk4 foreign key (test_table_3_fk4) references test_table_3 (test_pk) on update set null on delete cascade
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table test_table_5 ( test_column_1 integer )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_6 (
    test_pk integer not null,
    test_int integer not null,
    constraint test_table_6_pk primary key (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_7 (
    test_pk integer not null,
    test_table_6_fk1 integer not null,
    constraint test_table_7_pk primary key (test_pk),
    constraint fk_test_table_7_test_table_6_fk1 foreign key (test_table_6_fk1) references test_table_6 (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_8 (
    test_pk integer not null,
    test_table_8_fk1 integer not null,
    constraint test_table_8_pk primary key (test_pk),
    constraint fk_test_table_8_test_table_6_fk1 foreign key (test_table_8_fk1) references test_table_6 (test_pk),
    constraint test_table_8_uk1 unique (test_table_8_fk1)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_9 (
    test_pk integer not null,
    test_table_9_fk1 integer not null,
    constraint test_table_9_pk primary key (test_pk),
    constraint fk_test_table_9_test_table_6_fk1 foreign key (test_table_9_fk1) references test_table_6 (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create unique index ux_test_table_9_fk1 on test_table_9 (test_table_9_fk1)", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop table test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_4", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_3", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_5", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_7", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_8", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_9", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_6", CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task RenderTables_GivenSimpleTable_GeneratesExpectedDot()
        {
            var table = await GetTable("test_table_5").ConfigureAwait(false);
            var tables = new[] { table };

            var result = Formatter.RenderTables(tables);

            Assert.That(result, Is.EqualTo(TestTable5Dot).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task RenderTables_GivenComplexTable_GeneratesExpectedDot()
        {
            var table = await GetTable("test_table_1").ConfigureAwait(false);
            var tables = new[] { table };

            var result = Formatter.RenderTables(tables);

            Assert.That(result, Is.EqualTo(TestTable1Dot).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task RenderTables_GivenTableWithMultipleIndexes_GeneratesExpectedDot()
        {
            var table = await GetTable("test_table_2").ConfigureAwait(false);
            var tables = new[] { table };

            var result = Formatter.RenderTables(tables);

            Assert.That(result, Is.EqualTo(TestTable2Dot).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task RenderTables_GivenTablesWithMultipleRelationalKeys_GeneratesExpectedDot()
        {
            var tables = new[]
            {
                await GetTable("test_table_6").ConfigureAwait(false),
                await GetTable("test_table_7").ConfigureAwait(false),
                await GetTable("test_table_8").ConfigureAwait(false),
                await GetTable("test_table_9").ConfigureAwait(false)
            };

            var result = Formatter.RenderTables(tables);

            Assert.That(result, Is.EqualTo(MultipleRelationshipsDot).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        private const string TestTable1Dot = @"// Schematic version 1.0.0.0
digraph ""unnamed graph"" {
  graph [
    rankdir=RL
    ratio=compress
    bgcolor=""#FFFFFF""
  ]
  node [
    fontname=""Courier""
    shape=none
  ]
  edge [
    arrowhead=open
  ]
  ""test-table-1-8dbbc0e3"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_1</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_int</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_nullable_int</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_numeric</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_nullable_numeric</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_blob</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_datetime</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_string</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_string_with_default</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_1_pk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#EFEBA8""><FONT FACE=""Helvetica""><B>Primary Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#EFEBA8""><B>test_table_1_pk</B></TD></TR><TR><TD ALIGN=""LEFT"">test_pk</TD></TR></TABLE></TD></TR></TABLE>
    >
    tooltip=""main.test_table_1""
  ]
}
";

        private const string TestTable2Dot = @"// Schematic version 1.0.0.0
digraph ""unnamed graph"" {
  graph [
    rankdir=RL
    ratio=compress
    bgcolor=""#FFFFFF""
  ]
  node [
    fontname=""Courier""
    shape=none
  ]
  edge [
    arrowhead=open
  ]
  ""test-table-2-8f8e07fc"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_2</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk_1</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk_2</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">first_name</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">middle_name</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">last_name</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">comment</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_2_pk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#EFEBA8""><FONT FACE=""Helvetica""><B>Primary Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#EFEBA8""><B>test_table_2_pk</B></TD></TR><TR><TD ALIGN=""LEFT"">test_pk_1</TD></TR><TR><TD ALIGN=""LEFT"">test_pk_2</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_2_multi_uk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#B8D0DD""><FONT FACE=""Helvetica""><B>Unique Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#B8D0DD""><B>test_table_2_multi_uk</B></TD></TR><TR><TD ALIGN=""LEFT"">first_name</TD></TR><TR><TD ALIGN=""LEFT"">middle_name</TD></TR><TR><TD ALIGN=""LEFT"">last_name</TD></TR></TABLE></TD></TR></TABLE>
    >
    tooltip=""main.test_table_2""
  ]
}
";

        private const string TestTable5Dot = @"// Schematic version 1.0.0.0
digraph ""unnamed graph"" {
  graph [
    rankdir=RL
    ratio=compress
    bgcolor=""#FFFFFF""
  ]
  node [
    fontname=""Courier""
    shape=none
  ]
  edge [
    arrowhead=open
  ]
  ""test-table-5-f256cb03"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_5</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_column_1</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD></TR></TABLE>
    >
    tooltip=""main.test_table_5""
  ]
}
";

        private const string MultipleRelationshipsDot = @"// Schematic version 1.0.0.0
digraph ""unnamed graph"" {
  graph [
    rankdir=RL
    ratio=compress
    bgcolor=""#FFFFFF""
  ]
  node [
    fontname=""Courier""
    shape=none
  ]
  edge [
    arrowhead=open
  ]
  ""test-table-6-06a4affa"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_6</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_int</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica"">3 C</FONT></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_6_pk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#EFEBA8""><FONT FACE=""Helvetica""><B>Primary Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#EFEBA8""><B>test_table_6_pk</B></TD></TR><TR><TD ALIGN=""LEFT"">test_pk</TD></TR></TABLE></TD></TR></TABLE>
    >
    tooltip=""main.test_table_6""
  ]
  ""test-table-7-85e75413"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_7</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_table_6_fk1</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica"">1 P</FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_7_pk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#EFEBA8""><FONT FACE=""Helvetica""><B>Primary Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#EFEBA8""><B>test_table_7_pk</B></TD></TR><TR><TD ALIGN=""LEFT"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""fk_test_table_7_test_table_6_fk1""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#E5E5E5""><FONT FACE=""Helvetica""><B>Foreign Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#E5E5E5""><B>fk_test_table_7_test_table_6_fk1</B></TD></TR><TR><TD ALIGN=""LEFT"">test_table_6_fk1</TD></TR></TABLE></TD></TR></TABLE>
    >
    tooltip=""main.test_table_7""
  ]
  ""test-table-8-987241f1"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_8</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_table_8_fk1</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica"">1 P</FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_8_pk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#EFEBA8""><FONT FACE=""Helvetica""><B>Primary Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#EFEBA8""><B>test_table_8_pk</B></TD></TR><TR><TD ALIGN=""LEFT"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_8_uk1""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#B8D0DD""><FONT FACE=""Helvetica""><B>Unique Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#B8D0DD""><B>test_table_8_uk1</B></TD></TR><TR><TD ALIGN=""LEFT"">test_table_8_fk1</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""fk_test_table_8_test_table_6_fk1""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#E5E5E5""><FONT FACE=""Helvetica""><B>Foreign Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#E5E5E5""><B>fk_test_table_8_test_table_6_fk1</B></TD></TR><TR><TD ALIGN=""LEFT"">test_table_8_fk1</TD></TR></TABLE></TD></TR></TABLE>
    >
    tooltip=""main.test_table_8""
  ]
  ""test-table-9-1c63c09a"" [
    label=<
      <TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><FONT FACE=""Helvetica""><B>Table</B></FONT></TD></TR><TR><TD BGCOLOR=""#BFE3C6"" COLSPAN=""3""><B>main.test_table_9</B></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" ALIGN=""LEFT""><TABLE BORDER=""0"" CELLSPACING=""0"" ALIGN=""LEFT""><TR ALIGN=""LEFT""><TD ALIGN=""LEFT"" BGCOLOR=""#FFFFFF"">test_table_9_fk1</TD></TR></TABLE></TD></TR><TR><TD ALIGN=""LEFT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica"">1 P</FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""></FONT></TD><TD ALIGN=""RIGHT"" BGCOLOR=""#BFE3C6""><FONT FACE=""Helvetica""> </FONT></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""test_table_9_pk""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#EFEBA8""><FONT FACE=""Helvetica""><B>Primary Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#EFEBA8""><B>test_table_9_pk</B></TD></TR><TR><TD ALIGN=""LEFT"">test_pk</TD></TR></TABLE></TD></TR><TR><TD COLSPAN=""3"" CELLPADDING=""0"" BORDER=""0"" PORT=""fk_test_table_9_test_table_6_fk1""><TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" CELLPADDING=""4"" BGCOLOR=""#FFFFFF""><TR><TD BGCOLOR=""#E5E5E5""><FONT FACE=""Helvetica""><B>Foreign Key</B></FONT></TD></TR><TR><TD BGCOLOR=""#E5E5E5""><B>fk_test_table_9_test_table_6_fk1</B></TD></TR><TR><TD ALIGN=""LEFT"">test_table_9_fk1</TD></TR></TABLE></TD></TR></TABLE>
    >
    tooltip=""main.test_table_9""
  ]
  ""test-table-7-85e75413"":""fk_test_table_7_test_table_6_fk1"" -> ""test-table-6-06a4affa"":""test_table_6_pk""
  ""test-table-8-987241f1"":""fk_test_table_8_test_table_6_fk1"" -> ""test-table-6-06a4affa"":""test_table_6_pk""
  ""test-table-9-1c63c09a"":""fk_test_table_9_test_table_6_fk1"" -> ""test-table-6-06a4affa"":""test_table_6_pk""
}
";
    }
}
