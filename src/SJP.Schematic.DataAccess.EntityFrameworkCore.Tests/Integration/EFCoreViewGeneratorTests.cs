using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration;

internal sealed class EFCoreViewGeneratorTests : SqliteTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    private Task<IDatabaseView> GetView(Identifier viewName) => Database.GetView(viewName).UnwrapSomeAsync();

    private static IDatabaseViewGenerator ViewGenerator => new EFCoreViewGenerator(new MockFileSystem(), new PascalCaseNameTranslator(), TestNamespace);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
create table test_view_table_1 (
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
        await DbConnection.ExecuteAsync("create view test_view_1 as select * from test_view_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view test_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table test_view_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task Generate_GivenViewWithVariousColumnTypes_GeneratesExpectedOutput()
    {
        var view = await GetView("test_view_1").ConfigureAwait(false);
        var generator = ViewGenerator;

        var expected = TestView1Output;
        var result = generator.Generate(view, Option<IDatabaseViewComments>.None);

        Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    [Test]
    public async Task Generate_GivenViewWithViewAndColumnComments_GeneratesExpectedOutput()
    {
        const string viewComment = "This is a test view comment for EF Core";
        const string columnComment = "This is a test column comment for EF Core";

        var view = await GetView("test_view_1").ConfigureAwait(false);
        var generator = ViewGenerator;

        var comment = new DatabaseViewComments(
            "test_view_1",
            Option<string>.Some(viewComment),
            new Dictionary<Identifier, Option<string>> { ["test_int"] = Option<string>.Some(columnComment) }
        );
        var result = generator.Generate(view, comment);

        var expected = TestView1WithCommentOutput;
        Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    [Test]
    public async Task Generate_GivenMultiLineViewWithViewAndColumnComments_GeneratesExpectedOutput()
    {
        const string viewComment = @"This is a test view comment for EF Core.

This is a second line for it.";
        const string columnComment = @"This is a test column comment for EF Core.

This is a second line for it.";

        var view = await GetView("test_view_1").ConfigureAwait(false);
        var generator = ViewGenerator;

        var comment = new DatabaseViewComments(
            "test_view_1",
            Option<string>.Some(viewComment),
            new Dictionary<Identifier, Option<string>> { ["test_int"] = Option<string>.Some(columnComment) }
        );
        var result = generator.Generate(view, comment);

        var expected = TestView1MultiLineOutput;
        Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
    }

    private const string TestNamespace = "EFCoreTestNamespace";

    private readonly string TestView1Output = @"using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_1</c> view.
    /// </summary>
    public record TestView1
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long? TestPk { get; set; }

        /// <summary>
        /// The <c>test_int</c> column.
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long? TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Column(""test_nullable_int"", TypeName = ""INTEGER"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Column(""test_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Column(""test_nullable_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Column(""test_blob"", TypeName = ""BLOB"")]
        public byte[]? TestBlob { get; set; }

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

    private readonly string TestView1WithCommentOutput = @"using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// This is a test view comment for EF Core
    /// </summary>
    public record TestView1
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long? TestPk { get; set; }

        /// <summary>
        /// This is a test column comment for EF Core
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long? TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Column(""test_nullable_int"", TypeName = ""INTEGER"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Column(""test_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Column(""test_nullable_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Column(""test_blob"", TypeName = ""BLOB"")]
        public byte[]? TestBlob { get; set; }

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

    private readonly string TestView1MultiLineOutput = @"using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test view comment for EF Core.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    public record TestView1
    {
        /// <summary>
        /// The <c>test_pk</c> column.
        /// </summary>
        [Column(""test_pk"", TypeName = ""INTEGER"")]
        public long? TestPk { get; set; }

        /// <summary>
        /// <para>This is a test column comment for EF Core.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        [Column(""test_int"", TypeName = ""INTEGER"")]
        public long? TestInt { get; set; }

        /// <summary>
        /// The <c>test_nullable_int</c> column.
        /// </summary>
        [Column(""test_nullable_int"", TypeName = ""INTEGER"")]
        public long? TestNullableInt { get; set; }

        /// <summary>
        /// The <c>test_numeric</c> column.
        /// </summary>
        [Column(""test_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNumeric { get; set; }

        /// <summary>
        /// The <c>test_nullable_numeric</c> column.
        /// </summary>
        [Column(""test_nullable_numeric"", TypeName = ""NUMERIC"")]
        public decimal? TestNullableNumeric { get; set; }

        /// <summary>
        /// The <c>test_blob</c> column.
        /// </summary>
        [Column(""test_blob"", TypeName = ""BLOB"")]
        public byte[]? TestBlob { get; set; }

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
}