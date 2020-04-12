using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration
{
    internal sealed class OrmLiteTableGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private static IDatabaseTableGenerator TableGenerator => new OrmLiteTableGenerator(new PascalCaseNameTranslator(), TestNamespace);

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
    constraint test_table_2_single_uk unique (middle_name),
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
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop table test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_4", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_3", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_5", CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task Generate_GivenTableWithVariousColumnTypes_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_1").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable1Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task Generate_GivenTableWithVariousIndexesAndConstraints_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_2").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable2Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task Generate_GivenTableWithForeignKeys_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_4").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable4Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task Generate_GivenTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for OrmLite";
            const string columnComment = "This is a test column comment for OrmLite";

            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_5").ConfigureAwait(false);
            var generator = TableGenerator;

            var comment = new RelationalDatabaseTableComments("test_table_5",
                Option<string>.Some(tableComment),
                Option<string>.None,
                new Dictionary<Identifier, Option<string>> { ["test_column_1"] = Option<string>.Some(columnComment) },
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );
            var result = generator.Generate(tables, table, comment);

            var expected = TestTable5Output;
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for OrmLite.

This is a second line for it.";
            const string columnComment = @"This is a test column comment for OrmLite.

This is a second line for it.";

            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_5").ConfigureAwait(false);
            var generator = TableGenerator;

            var comment = new RelationalDatabaseTableComments("test_table_5",
                Option<string>.Some(tableComment),
                Option<string>.None,
                new Dictionary<Identifier, Option<string>> { ["test_column_1"] = Option<string>.Some(columnComment) },
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );
            var result = generator.Generate(tables, table, comment);

            var expected = TestTable5MultiLineOutput;
            Assert.That(result, Is.EqualTo(expected));
        }

        private const string TestNamespace = "OrmLiteTestNamespace";

        private readonly string TestTable1Output = @"using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_1</c> table.
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_table_1"")]
    public class TestTable1
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        [Alias(""test_pk"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Alias(""test_int"")]
        public long TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Alias(""test_nullable_int"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Alias(""test_numeric"")]
        public decimal TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Alias(""test_nullable_numeric"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Required]
        [Alias(""test_blob"")]
        public byte[] TestBlob { get; set; } = default!;

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Default(""CURRENT_TIMESTAMP"")]
        [Alias(""test_datetime"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Alias(""test_string"")]
        public string? TestString { get; set; }

        /// <summary>
        /// The <c>test_string_with_default</c> column.
        /// </summary>
        [Default(""'test'"")]
        [Alias(""test_string_with_default"")]
        public decimal? TestStringWithDefault { get; set; }
    }
}";

        private readonly string TestTable2Output = @"using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_2</c> table.
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_table_2"")]
    [UniqueConstraint(nameof(FirstName), nameof(MiddleName), nameof(LastName))]
    [CompositeIndex(true, nameof(FirstName), nameof(MiddleName))]
    [CompositeIndex(nameof(FirstName), nameof(LastName))]
    public class TestTable2
    {
        /// <summary>
        /// The <c>test_pk_1</c> column.
        /// </summary>
        [Alias(""test_pk_1"")]
        public long TestPk1 { get; set; }

        /// <summary>
        /// The <c>test_pk_2</c> column.
        /// </summary>
        [Alias(""test_pk_2"")]
        public long TestPk2 { get; set; }

        /// <summary>
        /// The <c>first_name</c> column.
        /// </summary>
        [Required]
        [Alias(""first_name"")]
        public string FirstName { get; set; } = default!;

        /// <summary>
        /// The <c>middle_name</c> column.
        /// </summary>
        [Required]
        [Unique]
        [Alias(""middle_name"")]
        public string MiddleName { get; set; } = default!;

        /// <summary>
        /// The <c>last_name</c> column.
        /// </summary>
        [Required]
        [Index(true)]
        [Alias(""last_name"")]
        public string LastName { get; set; } = default!;

        /// <summary>
        /// The <c>comment</c> column.
        /// </summary>
        [Index]
        [Alias(""comment"")]
        public string? Comment { get; set; }
    }
}";

        private readonly string TestTable4Output = @"using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_4</c> table.
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_table_4"")]
    public class TestTable4
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [PrimaryKey]
        [AutoIncrement]
        [Alias(""test_pk"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Alias(""test_int"")]
        public long TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Alias(""test_nullable_int"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Alias(""test_numeric"")]
        public decimal TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Alias(""test_nullable_numeric"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Required]
        [Alias(""test_blob"")]
        public byte[] TestBlob { get; set; } = default!;

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Default(""CURRENT_TIMESTAMP"")]
        [Alias(""test_datetime"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Alias(""test_string"")]
        public string? TestString { get; set; }

        /// <summary>
        /// The <c>test_string_with_default</c> column.
        /// </summary>
        [Default(""'test'"")]
        [Alias(""test_string_with_default"")]
        public decimal? TestStringWithDefault { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk1</c> column.
        /// </summary>
        [ForeignKey(typeof(TestTable3), ForeignKeyName = ""fk_test_table_4_test_table_3_fk1"")]
        [Alias(""test_table_3_fk1"")]
        public long? TestTable3Fk1 { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk2</c> column.
        /// </summary>
        [ForeignKey(typeof(TestTable3), ForeignKeyName = ""fk_test_table_4_test_table_3_fk1"", OnUpdate = ""CASCADE"")]
        [Alias(""test_table_3_fk2"")]
        public long? TestTable3Fk2 { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk3</c> column.
        /// </summary>
        [ForeignKey(typeof(TestTable3), ForeignKeyName = ""fk_test_table_4_test_table_3_fk1"", OnDelete = ""SET NULL"")]
        [Alias(""test_table_3_fk3"")]
        public long? TestTable3Fk3 { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk4</c> column.
        /// </summary>
        [ForeignKey(typeof(TestTable3), ForeignKeyName = ""fk_test_table_4_test_table_3_fk1"", OnDelete = ""CASCADE"", OnUpdate = ""SET NULL"")]
        [Alias(""test_table_3_fk4"")]
        public long? TestTable3Fk4 { get; set; }
    }
}";

        private readonly string TestTable5Output = @"using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// This is a test table comment for OrmLite
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_table_5"")]
    public class TestTable5
    {
        /// <summary>
        /// This is a test column comment for OrmLite
        /// </summary>
        [Alias(""test_column_1"")]
        public long? TestColumn1 { get; set; }
    }
}";

        private readonly string TestTable5MultiLineOutput = @"using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test table comment for OrmLite.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    [Schema(""main"")]
    [Alias(""test_table_5"")]
    public class TestTable5
    {
        /// <summary>
        /// <para>This is a test column comment for OrmLite.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        [Alias(""test_column_1"")]
        public long? TestColumn1 { get; set; }
    }
}";
    }
}
