using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration
{
    internal sealed class EFCoreTableGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private static IDatabaseTableGenerator TableGenerator => new EFCoreTableGenerator(new PascalCaseNameTranslator(), TestNamespace);

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
    test_string_with_default default 'test'
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
    test_string_with_default default 'test'
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
    test_string_with_default default 'test',
    test_table_3_fk1 integer,
    test_table_3_fk2 integer,
    test_table_3_fk3 integer,
    test_table_3_fk4 integer,
    constraint fk_test_table_4_test_table_3_fk1 foreign key (test_table_3_fk1) references test_table_3 (test_pk),
    constraint fk_test_table_4_test_table_3_fk2 foreign key (test_table_3_fk2) references test_table_3 (test_pk) on update cascade,
    constraint fk_test_table_4_test_table_3_fk3 foreign key (test_table_3_fk3) references test_table_3 (test_pk) on delete set null,
    constraint fk_test_table_4_test_table_3_fk4 foreign key (test_table_3_fk4) references test_table_3 (test_pk) on update set null on delete cascade
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table test_table_5 ( test_column_1 integer )").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table test_table_6 (
    test_pk integer not null primary key autoincrement,
    test_int integer not null
)").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
create table test_table_7 (
    test_pk integer not null primary key autoincrement,
    test_table_6_fk1 integer not null,
    constraint fk_test_table_7_test_table_6_fk1 foreign key (test_table_6_fk1) references test_table_6 (test_pk)
)").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_4").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_3").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_5").ConfigureAwait(false);
        }

        [Test]
        public async Task Generate_GivenTableWithVariousColumnTypes_GeneratesExpectedOutput()
        {
            var table = await GetTable("test_table_1").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable1Output;
            var result = generator.Generate(table, Option<IRelationalDatabaseTableComments>.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenTableWithVariousIndexesAndConstraints_GeneratesExpectedOutput()
        {
            var table = await GetTable("test_table_2").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable2Output;
            var result = generator.Generate(table, Option<IRelationalDatabaseTableComments>.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenTableWithChildKeys_GeneratesExpectedOutput()
        {
            var table = await GetTable("test_table_3").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable3Output;
            var result = generator.Generate(table, Option<IRelationalDatabaseTableComments>.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenTableWithForeignKeys_GeneratesExpectedOutput()
        {
            var table = await GetTable("test_table_4").ConfigureAwait(false);
            var generator = TableGenerator;

            var expected = TestTable4Output;
            var result = generator.Generate(table, Option<IRelationalDatabaseTableComments>.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for EF Core";
            const string columnComment = "This is a test column comment for EF Core";

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
            var result = generator.Generate(table, comment);

            var expected = TestTable5Output;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for EF Core.

This is a second line for it.";
            const string columnComment = @"This is a test column comment for EF Core.

This is a second line for it.";

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
            var result = generator.Generate(table, comment);

            var expected = TestTable5MultiLineOutput;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenTableWithForeignKeyComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for EF Core";
            const string foreignKeyComment = "This is a test foreign key comment for EF Core";

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
            var result = generator.Generate(table, comment);

            var expected = TestTable7Output;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithForeignKeyComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for EF Core.

This is a second line for it.";
            const string foreignKeyComment = @"This is a test foreign key comment for EF Core.

This is a second line for it.";

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
            var result = generator.Generate(table, comment);

            var expected = TestTable7MultiLineOutput;
            Assert.AreEqual(expected, result);
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
    public class TestTable1
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
        public byte[] TestBlob { get; set; }

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Column(""test_datetime"", TypeName = ""NUMERIC"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Column(""test_string"", TypeName = ""TEXT"")]
        public string TestString { get; set; }

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
    public class TestTable2
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
        public string FirstName { get; set; }

        /// <summary>
        /// The <c>middle_name</c> column.
        /// </summary>
        [Required]
        [Column(""middle_name"", TypeName = ""TEXT"")]
        public string MiddleName { get; set; }

        /// <summary>
        /// The <c>last_name</c> column.
        /// </summary>
        [Required]
        [Column(""last_name"", TypeName = ""TEXT"")]
        public string LastName { get; set; }

        /// <summary>
        /// The <c>comment</c> column.
        /// </summary>
        [Column(""comment"", TypeName = ""TEXT"")]
        public string Comment { get; set; }
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
    public class TestTable3
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
        public byte[] TestBlob { get; set; }

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Column(""test_datetime"", TypeName = ""NUMERIC"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Column(""test_string"", TypeName = ""TEXT"")]
        public string TestString { get; set; }

        /// <summary>
        /// The <c>test_string_with_default</c> column.
        /// </summary>
        [Column(""test_string_with_default"", TypeName = ""NUMERIC"")]
        public decimal? TestStringWithDefault { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c> entities.
        /// </summary>
        public virtual ICollection<main.TestTable4> TestTable4s { get; set; } = new HashSet<main.TestTable4>();

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c> entities.
        /// </summary>
        public virtual ICollection<main.TestTable4> TestTable4s { get; set; } = new HashSet<main.TestTable4>();

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c> entities.
        /// </summary>
        public virtual ICollection<main.TestTable4> TestTable4s { get; set; } = new HashSet<main.TestTable4>();

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> child key. Navigates from <c>test_table_3</c> to <c>test_table_4</c> entities.
        /// </summary>
        public virtual ICollection<main.TestTable4> TestTable4s { get; set; } = new HashSet<main.TestTable4>();
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
    public class TestTable4
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
        public byte[] TestBlob { get; set; }

        /// <summary>
        /// The <c>test_datetime</c> column.
        /// </summary>
        [Column(""test_datetime"", TypeName = ""NUMERIC"")]
        public decimal? TestDatetime { get; set; }

        /// <summary>
        /// The <c>test_string</c> column.
        /// </summary>
        [Column(""test_string"", TypeName = ""TEXT"")]
        public string TestString { get; set; }

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
        public virtual main.TestTable3 TestTable3 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual main.TestTable3 TestTable3 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual main.TestTable3 TestTable3 { get; set; }

        /// <summary>
        /// The <c>fk_test_table_4_test_table_3_fk1</c> foreign key. Navigates from <c>test_table_4</c> to <c>test_table_3</c>.
        /// </summary>
        public virtual main.TestTable3 TestTable3 { get; set; }
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
    public class TestTable5
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
    public class TestTable5
    {
        /// <summary>
        /// <para>This is a test column comment for EF Core.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        [Column(""test_column_1"", TypeName = ""INTEGER"")]
        public long? TestColumn1 { get; set; }
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
    public class TestTable7
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
        public virtual main.TestTable6 TestTable6 { get; set; }
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
    public class TestTable7
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
        public virtual main.TestTable6 TestTable6 { get; set; }
    }
}";
    }
}
