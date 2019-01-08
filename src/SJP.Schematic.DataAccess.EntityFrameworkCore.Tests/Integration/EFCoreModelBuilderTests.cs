using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration
{
    internal sealed class EFCoreModelBuilderTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        private static EFCoreModelBuilder Builder => new EFCoreModelBuilder(new PascalCaseNameTranslator(), "    ", "  ");

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
create table test_table_1 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'asd'
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table test_table_2 (
    test_pk_1 integer not null,
    test_pk_2 integer not null,
    first_name text not null,
    middle_name text not null,
    last_name text not null,
    comment text null,
    constraint test_table_2_pk primary key (test_pk_1, test_pk_2),
    constraint test_table_2_multi_uk unique (first_name, middle_name, last_name)
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_2_first_name on test_table_2 (first_name, last_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create index ix_test_table_2_comment on test_table_2 (comment)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ux_test_table_2_first_name_middle_name on test_table_2 (first_name, middle_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create unique index ux_test_table_2_last_name on test_table_2 (last_name)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table test_table_3 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'asd'
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table test_table_4 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null,
    test_nullable_int integer,
    test_numeric numeric not null,
    test_nullable_numeric numeric,
    test_blob blob not null,
    test_datetime datetime default CURRENT_TIMESTAMP,
    test_string text,
    test_string_with_default default 'asd',
    test_table_3_fk1 integer,
    test_table_3_fk2 integer,
    test_table_3_fk3 integer,
    test_table_3_fk4 integer,
    constraint fk_test_table_4_test_table_3_fk1 foreign key (test_table_3_fk1) references test_table_3 (test_pk),
    constraint fk_test_table_4_test_table_3_fk2 foreign key (test_table_3_fk2) references test_table_3 (test_pk) on update cascade,
    constraint fk_test_table_4_test_table_3_fk3 foreign key (test_table_3_fk3) references test_table_3 (test_pk) on delete set null,
    constraint fk_test_table_4_test_table_3_fk4 foreign key (test_table_3_fk4) references test_table_3 (test_pk) on update set null on delete cascade
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_3").ConfigureAwait(false);
        }

        [Test]
        public async Task AddTable_GivenTable_ReturnsNotNullBuilder()
        {
            var database = Database;
            var tables = await database.GetAllTables().ConfigureAwait(false);
            var table = tables.First();

            var builder = Builder;
            var result = builder.AddTable(table);

            Assert.NotNull(result);
        }

        [Test]
        public static void HasRecords_GivenUnmodifiedBuilder_ReturnsFalse()
        {
            var builder = Builder;

            Assert.IsFalse(builder.HasRecords);
        }

        [Test]
        public async Task HasRecords_AddTableGivenTableRequiringAnnotation_ReturnsTrue()
        {
            var database = Database;
            var tables = await database.GetAllTables().ConfigureAwait(false);
            var table = tables.First();

            var builder = Builder;
            builder.AddTable(table);

            Assert.IsTrue(builder.HasRecords);
        }

        [Test]
        public async Task ToString_GivenTablesWithVariousAnnotationsRequired_GeneratesExpectedOutput()
        {
            var database = Database;

            var builder = Builder;
            var tables = await database.GetAllTables().ConfigureAwait(false);
            foreach (var table in tables)
                builder.AddTable(table);

            var expected = TestModelBuilderOutput;
            var result = builder.ToString();

            Assert.AreEqual(expected, result);
        }

        private readonly string TestModelBuilderOutput = @"    modelBuilder.Entity<Main.TestTable1>()
      .Property(t => t.TestDatetime)
      .HasDefaultValue(""CURRENT_TIMESTAMP"");
    modelBuilder.Entity<Main.TestTable1>()
      .Property(t => t.TestStringWithDefault)
      .HasDefaultValue(""'asd'"");
    modelBuilder.Entity<Main.TestTable1>()
      .HasKey(t => t.TestPk);
    modelBuilder.Entity<Main.TestTable2>()
      .HasKey(t => new { t.TestPk1, t.TestPk2 })
      .HasName(""test_table_2_pk"");
    modelBuilder.Entity<Main.TestTable2>()
      .HasAlternateKey(t => new { t.FirstName, t.MiddleName, t.LastName })
      .HasName(""test_table_2_multi_uk"");
    modelBuilder.Entity<Main.TestTable2>()
      .HasIndex(t => t.LastName)
      .IsUnique()
      .HasName(""ux_test_table_2_last_name"");
    modelBuilder.Entity<Main.TestTable2>()
      .HasIndex(t => new { t.FirstName, t.MiddleName })
      .IsUnique()
      .HasName(""ux_test_table_2_first_name_middle_name"");
    modelBuilder.Entity<Main.TestTable2>()
      .HasIndex(t => t.Comment)
      .HasName(""ix_test_table_2_comment"");
    modelBuilder.Entity<Main.TestTable2>()
      .HasIndex(t => new { t.FirstName, t.LastName })
      .HasName(""ix_test_table_2_first_name"");
    modelBuilder.Entity<Main.TestTable3>()
      .Property(t => t.TestDatetime)
      .HasDefaultValue(""CURRENT_TIMESTAMP"");
    modelBuilder.Entity<Main.TestTable3>()
      .Property(t => t.TestStringWithDefault)
      .HasDefaultValue(""'asd'"");
    modelBuilder.Entity<Main.TestTable3>()
      .HasKey(t => t.TestPk);
    modelBuilder.Entity<Main.TestTable4>()
      .Property(t => t.TestDatetime)
      .HasDefaultValue(""CURRENT_TIMESTAMP"");
    modelBuilder.Entity<Main.TestTable4>()
      .Property(t => t.TestStringWithDefault)
      .HasDefaultValue(""'asd'"");
    modelBuilder.Entity<Main.TestTable4>()
      .HasKey(t => t.TestPk);
    modelBuilder.Entity<Main.TestTable4>()
      .HasOne(t => t.TestTable3)
      .WithMany(t => t.TestTable4s)
      .HasForeignKey(t => t.TestTable3Fk4)
      .HasPrincipalKey(t => t.TestPk)
      .HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
    modelBuilder.Entity<Main.TestTable4>()
      .HasOne(t => t.TestTable3)
      .WithMany(t => t.TestTable4s)
      .HasForeignKey(t => t.TestTable3Fk3)
      .HasPrincipalKey(t => t.TestPk)
      .HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
    modelBuilder.Entity<Main.TestTable4>()
      .HasOne(t => t.TestTable3)
      .WithMany(t => t.TestTable4s)
      .HasForeignKey(t => t.TestTable3Fk2)
      .HasPrincipalKey(t => t.TestPk)
      .HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
    modelBuilder.Entity<Main.TestTable4>()
      .HasOne(t => t.TestTable3)
      .WithMany(t => t.TestTable4s)
      .HasForeignKey(t => t.TestTable3Fk1)
      .HasPrincipalKey(t => t.TestPk)
      .HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
";
    }
}
