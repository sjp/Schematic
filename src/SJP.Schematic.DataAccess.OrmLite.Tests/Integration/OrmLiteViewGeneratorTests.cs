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

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration;

internal sealed class OrmLiteViewGeneratorTests : SqliteTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    private Task<IDatabaseView> GetView(Identifier viewName) => Database.GetView(viewName).UnwrapSomeAsync();

    private static IDatabaseViewGenerator ViewGenerator => new OrmLiteViewGenerator(new MockFileSystem(), new PascalCaseNameTranslator(), TestNamespace);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
create view test_view_1 as
select
    1 as testint,
    2.45 as testdouble,
    X'DEADBEEF' as testblob,
    CURRENT_TIMESTAMP as testdatetime,
    'test' as teststring
", CancellationToken.None);
        await DbConnection.ExecuteAsync(@"create table view_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)", CancellationToken.None);
        await DbConnection.ExecuteAsync("create view test_view_2 as select * from view_test_table_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("create view test_view_3 as select 1 as test_column_1", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view test_view_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop view test_view_2", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table view_test_table_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop view test_view_3", CancellationToken.None);
    }

    [Test]
    public async Task Generate_GivenViewWithLiteralColumnTypes_GeneratesExpectedOutput()
    {
        var view = await GetView("test_view_1");
        var generator = ViewGenerator;

        var expected = TestView1Output;
        var result = generator.Generate(view, Option<IDatabaseViewComments>.None);

        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    [Test]
    public async Task Generate_GivenViewSelectingFromTable_GeneratesExpectedOutput()
    {
        var view = await GetView("test_view_2");
        var generator = ViewGenerator;

        var expected = TestView2Output;
        var result = generator.Generate(view, Option<IDatabaseViewComments>.None);

        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    [Test]
    public async Task Generate_GivenViewWithViewAndColumnComments_GeneratesExpectedOutput()
    {
        const string viewComment = "This is a test view comment for OrmLite";
        const string columnComment = "This is a test column comment for OrmLite";

        var view = await GetView("test_view_3");
        var generator = ViewGenerator;

        var comment = new DatabaseViewComments("test_view_3",
            Option<string>.Some(viewComment),
            new Dictionary<Identifier, Option<string>> { ["test_column_1"] = Option<string>.Some(columnComment) }
        );
        var result = generator.Generate(view, comment);

        var expected = TestView3Output;
        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    [Test]
    public async Task Generate_GivenMultiLineViewWithViewAndColumnComments_GeneratesExpectedOutput()
    {
        const string viewComment = @"This is a test view comment for OrmLite.

This is a second line for it.";
        const string columnComment = @"This is a test column comment for OrmLite.

This is a second line for it.";

        var view = await GetView("test_view_3");
        var generator = ViewGenerator;

        var comment = new DatabaseViewComments("test_view_3",
            Option<string>.Some(viewComment),
            new Dictionary<Identifier, Option<string>> { ["test_column_1"] = Option<string>.Some(columnComment) }
        );
        var result = generator.Generate(view, comment);

        var expected = TestView3MultiLineOutput;
        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    private const string TestNamespace = "OrmLiteTestNamespace";

    private readonly string TestView1Output = """
using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_1</c> view.
    /// </summary>
    [Schema("main")]
    [Alias("test_view_1")]
    public sealed record TestView1
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        [Alias("testint")]
        public long? Testint { get; set; }

        /// <summary>
        /// The <c>testdouble</c> column.
        /// </summary>
        [Alias("testdouble")]
        public double? Testdouble { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        [Alias("testblob")]
        public byte[]? Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        [Alias("testdatetime")]
        public string? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        [Alias("teststring")]
        public string? Teststring { get; set; }
    }
}
""";

    private readonly string TestView2Output = """
using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_2</c> view.
    /// </summary>
    [Schema("main")]
    [Alias("test_view_2")]
    public sealed record TestView2
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        [Alias("testint")]
        public long? Testint { get; set; }

        /// <summary>
        /// The <c>testdecimal</c> column.
        /// </summary>
        [Alias("testdecimal")]
        public decimal? Testdecimal { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        [Alias("testblob")]
        public byte[]? Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        [Alias("testdatetime")]
        public decimal? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        [Alias("teststring")]
        public string? Teststring { get; set; }
    }
}
""";

    private readonly string TestView3Output = """
using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// This is a test view comment for OrmLite
    /// </summary>
    [Schema("main")]
    [Alias("test_view_3")]
    public sealed record TestView3
    {
        /// <summary>
        /// This is a test column comment for OrmLite
        /// </summary>
        [Alias("test_column_1")]
        public long? TestColumn1 { get; set; }
    }
}
""";

    private readonly string TestView3MultiLineOutput = """
using System;
using ServiceStack.DataAnnotations;

namespace OrmLiteTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test view comment for OrmLite.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    [Schema("main")]
    [Alias("test_view_3")]
    public sealed record TestView3
    {
        /// <summary>
        /// <para>This is a test column comment for OrmLite.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        [Alias("test_column_1")]
        public long? TestColumn1 { get; set; }
    }
}
""";
}