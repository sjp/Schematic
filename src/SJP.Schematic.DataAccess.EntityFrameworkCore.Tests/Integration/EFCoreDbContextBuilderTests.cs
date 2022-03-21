using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration;

internal sealed class EFCoreDbContextBuilderTests : SqliteTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    private static EFCoreDbContextBuilder Builder => new(new PascalCaseNameTranslator(), "EFCoreTestNamespace");

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
create table test_table_1 (
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
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_4", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_table_3", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task ToString_GivenVariousTablesWithAnnotationsRequired_GeneratesExpectedOutput()
    {
        var builder = Builder;
        var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
        var views = await Database.GetAllViews().ToListAsync().ConfigureAwait(false);
        var sequences = await Database.GetAllSequences().ToListAsync().ConfigureAwait(false);

        var expected = TestAppContextOutput;
        var result = builder.Generate(tables, views, sequences);

        Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
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
        public DbSet<Main.TestTable1> TestTable1s { get; set; } = default!;

        /// <summary>
        /// Accesses the <c>main.test_table_2</c> table.
        /// </summary>
        public DbSet<Main.TestTable2> TestTable2s { get; set; } = default!;

        /// <summary>
        /// Accesses the <c>main.test_table_3</c> table.
        /// </summary>
        public DbSet<Main.TestTable3> TestTable3s { get; set; } = default!;

        /// <summary>
        /// Accesses the <c>main.test_table_4</c> table.
        /// </summary>
        public DbSet<Main.TestTable4> TestTable4s { get; set; } = default!;

        /// <summary>
        /// Configure the model that was discovered by convention from the defined entity types.
        /// </summary>
        /// <param name=""modelBuilder"">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Main.TestTable1>().Property(t => t.TestDatetime).HasDefaultValue(""CURRENT_TIMESTAMP"");
            modelBuilder.Entity<Main.TestTable1>().Property(t => t.TestStringWithDefault).HasDefaultValue(""'test'"");
            modelBuilder.Entity<Main.TestTable1>().HasKey(t => t.TestPk);
            modelBuilder.Entity<Main.TestTable2>().HasKey(t => new { t.TestPk1, t.TestPk2 }).HasName(""test_table_2_pk"");
            modelBuilder.Entity<Main.TestTable2>().HasAlternateKey(t => new { t.FirstName, t.MiddleName, t.LastName }).HasName(""test_table_2_multi_uk"");
            modelBuilder.Entity<Main.TestTable2>().HasIndex(t => t.LastName).IsUnique().HasDatabaseName(""ux_test_table_2_last_name"");
            modelBuilder.Entity<Main.TestTable2>().HasIndex(t => new { t.FirstName, t.MiddleName }).IsUnique().HasDatabaseName(""ux_test_table_2_first_name_middle_name"");
            modelBuilder.Entity<Main.TestTable2>().HasIndex(t => t.Comment).HasDatabaseName(""ix_test_table_2_comment"");
            modelBuilder.Entity<Main.TestTable2>().HasIndex(t => new { t.FirstName, t.LastName }).HasDatabaseName(""ix_test_table_2_first_name"");
            modelBuilder.Entity<Main.TestTable3>().Property(t => t.TestDatetime).HasDefaultValue(""CURRENT_TIMESTAMP"");
            modelBuilder.Entity<Main.TestTable3>().Property(t => t.TestStringWithDefault).HasDefaultValue(""'test'"");
            modelBuilder.Entity<Main.TestTable3>().HasKey(t => t.TestPk);
            modelBuilder.Entity<Main.TestTable4>().Property(t => t.TestDatetime).HasDefaultValue(""CURRENT_TIMESTAMP"");
            modelBuilder.Entity<Main.TestTable4>().Property(t => t.TestStringWithDefault).HasDefaultValue(""'test'"");
            modelBuilder.Entity<Main.TestTable4>().HasKey(t => t.TestPk);
            modelBuilder.Entity<Main.TestTable4>().HasOne(t => t.TestTable3).WithMany(t => t!.TestTable4s).HasForeignKey(t => t.TestTable3Fk4).HasPrincipalKey(t => t!.TestPk).HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
            modelBuilder.Entity<Main.TestTable4>().HasOne(t => t.TestTable3).WithMany(t => t!.TestTable4s).HasForeignKey(t => t.TestTable3Fk3).HasPrincipalKey(t => t!.TestPk).HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
            modelBuilder.Entity<Main.TestTable4>().HasOne(t => t.TestTable3).WithMany(t => t!.TestTable4s).HasForeignKey(t => t.TestTable3Fk2).HasPrincipalKey(t => t!.TestPk).HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
            modelBuilder.Entity<Main.TestTable4>().HasOne(t => t.TestTable3).WithMany(t => t!.TestTable4s).HasForeignKey(t => t.TestTable3Fk1).HasPrincipalKey(t => t!.TestPk).HasConstraintName(""fk_test_table_4_test_table_3_fk1"");
        }
    }
}";
}