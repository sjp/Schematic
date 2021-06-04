using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
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

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration
{
    internal sealed class EFCoreTableGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private static IDatabaseTableGenerator TableGenerator => new EFCoreTableGenerator(new MockFileSystem(), new PascalCaseNameTranslator(), TestNamespace);

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
            await DbConnection.ExecuteAsync("create table test_table_5 ( test_column_1 integer )", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_6 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_7 (
    test_pk integer not null primary key autoincrement,
    test_table_6_fk1 integer not null,
    constraint fk_test_table_7_test_table_6_fk1 foreign key (test_table_6_fk1) references test_table_6 (test_pk)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_8 (
    test_pk integer not null primary key autoincrement,
    test_table_8_fk1 integer not null,
    constraint fk_test_table_8_test_table_6_fk1 foreign key (test_table_8_fk1) references test_table_6 (test_pk)
    constraint test_table_8_uk1 unique (test_table_8_fk1)
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
create table test_table_9 (
    test_pk integer not null primary key autoincrement,
    test_table_9_fk1 integer not null,
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
        public async Task Generate_GivenTableWithVariousColumnTypes_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_1").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable1Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithVariousIndexesAndConstraints_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_2").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable2Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithChildKeys_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_3").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable3Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithForeignKeys_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_4").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable4Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for EF Core";
            const string columnComment = "This is a test column comment for EF Core";

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
            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for EF Core.

This is a second line for it.";
            const string columnComment = @"This is a test column comment for EF Core.

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
            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithForeignKeyComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for EF Core";
            const string foreignKeyComment = "This is a test foreign key comment for EF Core";

            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_7").ConfigureAwait(false);
            var generator = TableGenerator;

            var comment = new RelationalDatabaseTableComments("test_table_7",
                Option<string>.Some(tableComment),
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                new Dictionary<Identifier, Option<string>> { ["fk_test_table_7_test_table_6_fk1"] = Option<string>.Some(foreignKeyComment) },
                Empty.CommentLookup,
                Empty.CommentLookup
            );
            var result = generator.Generate(tables, table, comment);

            var expected = TestTable7Output;
            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithForeignKeyComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for EF Core.

This is a second line for it.";
            const string foreignKeyComment = @"This is a test foreign key comment for EF Core.

This is a second line for it.";

            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_7").ConfigureAwait(false);
            var generator = TableGenerator;

            var comment = new RelationalDatabaseTableComments("test_table_7",
                Option<string>.Some(tableComment),
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                new Dictionary<Identifier, Option<string>> { ["fk_test_table_7_test_table_6_fk1"] = Option<string>.Some(foreignKeyComment) },
                Empty.CommentLookup,
                Empty.CommentLookup
            );
            var result = generator.Generate(tables, table, comment);

            var expected = TestTable7MultiLineOutput;
            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithUniqueChildKeys_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_6").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable6Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        private const string TestNamespace = "EFCoreTestNamespace";

        private readonly string TestTable1Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_1</c> table.
    /// </summary>
    [Table(""test_table_1"", Schema = ""main"")]
    public record TestTable1
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Column(""test_nullable_int"", TypeName = ""INTEGER"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Column(""test_numeric"", TypeName = ""NUMERIC"")]
        public decimal TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Column(""test_nullable_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Required]
        [Column(""test_blob"", TypeName = ""BLOB"")]
        public byte[] TestBlob { get; set; } = default!;

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Column(""test_datetime"", TypeName = ""NUMERIC"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Column(""test_string"", TypeName = ""TEXT"")]
        public string? TestString { get; set; }

        /// <summary>
        /// The <c>test_string_with_default</c> column.
        /// </summary>
        [Column(""test_string_with_default"", TypeName = ""NUMERIC"")]
        public decimal? TestStringWithDefault { get; set; }
    }
}";

        private readonly string TestTable2Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_2</c> table.
    /// </summary>
    [Table(""test_table_2"", Schema = ""main"")]
    public record TestTable2
    {
        /// <summary>
        /// The <c>test_pk_1</c> column.
        /// </summary>
        [Column(""test_pk_1"", TypeName = ""INTEGER"")]
        public long TestPk1 { get; set; }

        /// <summary>
        /// The <c>test_pk_2</c> column.
        /// </summary>
        [Column(""test_pk_2"", TypeName = ""INTEGER"")]
        public long TestPk2 { get; set; }

        /// <summary>
        /// The <c>first_name</c> column.
        /// </summary>
        [Required]
        [Column(""first_name"", TypeName = ""TEXT"")]
        public string FirstName { get; set; } = default!;

        /// <summary>
        /// The <c>middle_name</c> column.
        /// </summary>
        [Required]
        [Column(""middle_name"", TypeName = ""TEXT"")]
        public string MiddleName { get; set; } = default!;

        /// <summary>
        /// The <c>last_name</c> column.
        /// </summary>
        [Required]
        [Column(""last_name"", TypeName = ""TEXT"")]
        public string LastName { get; set; } = default!;

        /// <summary>
        /// The <c>comment</c> column.
        /// </summary>
        [Column(""comment"", TypeName = ""TEXT"")]
        public string? Comment { get; set; }
    }
}";

        private readonly string TestTable3Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_3</c> table.
    /// </summary>
    [Table(""test_table_3"", Schema = ""main"")]
    public record TestTable3
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Column(""test_nullable_int"", TypeName = ""INTEGER"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Column(""test_numeric"", TypeName = ""NUMERIC"")]
        public decimal TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Column(""test_nullable_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Required]
        [Column(""test_blob"", TypeName = ""BLOB"")]
        public byte[] TestBlob { get; set; } = default!;

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Column(""test_datetime"", TypeName = ""NUMERIC"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Column(""test_string"", TypeName = ""TEXT"")]
        public string? TestString { get; set; }

        /// <summary>
        /// The <c>test_string_with_default</c> column.
        /// </summary>
        [Column(""test_string_with_default"", TypeName = ""NUMERIC"")]
        public decimal? TestStringWithDefault { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c>.
        /// </summary>
        public virtual ICollection<Main.TestTable4> TestTable4s { get; set; } = new HashSet<Main.TestTable4>();

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c>.
        /// </summary>
        public virtual ICollection<Main.TestTable4> TestTable4s_1 { get; set; } = new HashSet<Main.TestTable4>();

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c>.
        /// </summary>
        public virtual ICollection<Main.TestTable4> TestTable4s_2 { get; set; } = new HashSet<Main.TestTable4>();

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c>.
        /// </summary>
        public virtual ICollection<Main.TestTable4> TestTable4s_3 { get; set; } = new HashSet<Main.TestTable4>();
    }
}";

        private readonly string TestTable4Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_4</c> table.
    /// </summary>
    [Table(""test_table_4"", Schema = ""main"")]
    public record TestTable4
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Column(""test_nullable_int"", TypeName = ""INTEGER"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Column(""test_numeric"", TypeName = ""NUMERIC"")]
        public decimal TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Column(""test_nullable_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Required]
        [Column(""test_blob"", TypeName = ""BLOB"")]
        public byte[] TestBlob { get; set; } = default!;

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Column(""test_datetime"", TypeName = ""NUMERIC"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Column(""test_string"", TypeName = ""TEXT"")]
        public string? TestString { get; set; }

        /// <summary>
        /// The <c>test_string_with_default</c> column.
        /// </summary>
        [Column(""test_string_with_default"", TypeName = ""NUMERIC"")]
        public decimal? TestStringWithDefault { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk1</c> column.
        /// </summary>
        [Column(""test_table_3_fk1"", TypeName = ""INTEGER"")]
        public long? TestTable3Fk1 { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk2</c> column.
        /// </summary>
        [Column(""test_table_3_fk2"", TypeName = ""INTEGER"")]
        public long? TestTable3Fk2 { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk3</c> column.
        /// </summary>
        [Column(""test_table_3_fk3"", TypeName = ""INTEGER"")]
        public long? TestTable3Fk3 { get; set; }

        /// <summary>
        /// The <c>test_table_3_fk4</c> column.
        /// </summary>
        [Column(""test_table_3_fk4"", TypeName = ""INTEGER"")]
        public long? TestTable3Fk4 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual Main.TestTable3? TestTable3 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual Main.TestTable3? TestTable3_1 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual Main.TestTable3? TestTable3_2 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual Main.TestTable3? TestTable3_3 { get; set; }
    }
}";

        private readonly string TestTable5Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// This is a test table comment for EF Core
    /// </summary>
    [Table(""test_table_5"", Schema = ""main"")]
    public record TestTable5
    {
        /// <summary>
        /// This is a test column comment for EF Core
        /// </summary>
        [Column(""test_column_1"", TypeName = ""INTEGER"")]
        public long? TestColumn1 { get; set; }
    }
}";

        private readonly string TestTable5MultiLineOutput = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test table comment for EF Core.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    [Table(""test_table_5"", Schema = ""main"")]
    public record TestTable5
    {
        /// <summary>
        /// <para>This is a test column comment for EF Core.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        [Column(""test_column_1"", TypeName = ""INTEGER"")]
        public long? TestColumn1 { get; set; }
    }
}";

        private readonly string TestTable6Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_6</c> table.
    /// </summary>
    [Table(""test_table_6"", Schema = ""main"")]
    public record TestTable6
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long TestInt { get; set; }

        /// <summary>
        /// The <c>fk_test_table_7_test_table_6_fk1</c> child key. Navigates from <c>test_table_6</c> to <c>test_table_7</c>.
        /// </summary>
        public virtual ICollection<Main.TestTable7> TestTable7s { get; set; } = new HashSet<Main.TestTable7>();

        /// <summary>
        /// The <c>fk_test_table_8_test_table_6_fk1</c> child key. Navigates from <c>test_table_6</c> to <c>test_table_8</c>.
        /// </summary>
        public virtual Main.TestTable8? TestTable8s { get; set; }

        /// <summary>
        /// The <c>fk_test_table_9_test_table_6_fk1</c> child key. Navigates from <c>test_table_6</c> to <c>test_table_9</c>.
        /// </summary>
        public virtual Main.TestTable9? TestTable9s { get; set; }
    }
}";

        private readonly string TestTable7Output = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// This is a test table comment for EF Core
    /// </summary>
    [Table(""test_table_7"", Schema = ""main"")]
    public record TestTable7
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_table_6_fk1</c> column.
        /// </summary>
        [Column(""test_table_6_fk1"", TypeName = ""INTEGER"")]
        public long TestTable6Fk1 { get; set; }

        /// <summary>
        /// This is a test foreign key comment for EF Core
        /// </summary>
        public virtual Main.TestTable6 TestTable6 { get; set; } = default!;
    }
}";

        private readonly string TestTable7MultiLineOutput = @"using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test table comment for EF Core.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    [Table(""test_table_7"", Schema = ""main"")]
    public record TestTable7
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long TestPk { get; set; }

        /// <summary>
        /// The <c>test_table_6_fk1</c> column.
        /// </summary>
        [Column(""test_table_6_fk1"", TypeName = ""INTEGER"")]
        public long TestTable6Fk1 { get; set; }

        /// <summary>
        /// <para>This is a test foreign key comment for EF Core.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        public virtual Main.TestTable6 TestTable6 { get; set; } = default!;
    }
}";
    }
}
