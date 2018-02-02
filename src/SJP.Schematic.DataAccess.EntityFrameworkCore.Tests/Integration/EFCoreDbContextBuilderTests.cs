using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration
{
    [TestFixture]
    internal class EFCoreDbContextBuilderTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

        private EFCoreDbContextBuilder Builder => new EFCoreDbContextBuilder(Database, new PascalCaseNameProvider(), "EFCoreTestNamespace");

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
        public void ToString_GivenVariousTablesWithAnnotationsRequired_GeneratesExpectedOutput()
        {
            var builder = Builder;

            var expected = TestAppContextOutput;
            var result = builder.Generate();

            Assert.AreEqual(expected, result);
        }

        private readonly string TestAppContextOutput = @"using System;
using Microsoft.EntityFrameworkCore;

namespace EFCoreTestNamespace
{
    public class AppContext : DbContext
    {
        /// <summary>
        /// Accesses the <c>main.test_table_1</c> table.
        /// </summary>
        public DbSet<Main.TestTable1> TestTable1s { get; set; }

        /// <summary>
        /// Accesses the <c>main.test_table_2</c> table.
        /// </summary>
        public DbSet<Main.TestTable2> TestTable2s { get; set; }

        /// <summary>
        /// Accesses the <c>main.test_table_3</c> table.
        /// </summary>
        public DbSet<Main.TestTable3> TestTable3s { get; set; }

        /// <summary>
        /// Accesses the <c>main.test_table_4</c> table.
        /// </summary>
        public DbSet<Main.TestTable4> TestTable4s { get; set; }

        /// <summary>
        /// Configure the model that was discovered by convention from the defined entity types.
        /// </summary>
        /// <param name=""modelBuilder"">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Main.TestTable1>()
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
        }
    }
}";
    }
}
