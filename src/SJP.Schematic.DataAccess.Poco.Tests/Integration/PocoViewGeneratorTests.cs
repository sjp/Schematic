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

namespace SJP.Schematic.DataAccess.Poco.Tests.Integration;

internal sealed class PocoViewGeneratorTests : SqliteTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    private Task<IDatabaseView> GetView(Identifier viewName) => Database.GetView(viewName).UnwrapSomeAsync();

    private static IDatabaseViewGenerator ViewGenerator => new PocoViewGenerator(new MockFileSystem(), new PascalCaseNameTranslator(), TestNamespace);

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
", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"create table view_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view test_view_2 as select * from view_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view test_view_3 as select 1 as test_column_1", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view test_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view test_view_2", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table view_test_table_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view test_view_3", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task Generate_GivenViewWithLiteralColumnTypes_GeneratesExpectedOutput()
    {
        var view = await GetView("test_view_1").ConfigureAwait(false);
        var generator = ViewGenerator;

        const string expected = TestView1Output;
        var result = generator.Generate(view, Option<IDatabaseViewComments>.None);

        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    [Test]
    public async Task Generate_GivenViewSelectingFromTable_GeneratesExpectedOutput()
    {
        var view = await GetView("test_view_2").ConfigureAwait(false);
        var generator = ViewGenerator;

        const string expected = TestView2Output;
        var result = generator.Generate(view, Option<IDatabaseViewComments>.None);

        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    [Test]
    public async Task Generate_GivenViewWithViewAndColumnComments_GeneratesExpectedOutput()
    {
        const string viewComment = "This is a test view comment for Poco";
        const string columnComment = "This is a test column comment for Poco";

        var view = await GetView("test_view_3").ConfigureAwait(false);
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
        const string viewComment = @"This is a test view comment for Poco.

This is a second line for it.";
        const string columnComment = @"This is a test column comment for Poco.

This is a second line for it.";

        var view = await GetView("test_view_3").ConfigureAwait(false);
        var generator = ViewGenerator;

        var comment = new DatabaseViewComments("test_view_3",
            Option<string>.Some(viewComment),
            new Dictionary<Identifier, Option<string>> { ["test_column_1"] = Option<string>.Some(columnComment) }
        );
        var result = generator.Generate(view, comment);

        var expected = TestView3MultiLineOutput;
        Assert.That(result, Is.EqualTo(expected).IgnoreLineEndingFormat);
    }

    private const string TestNamespace = "PocoTestNamespace";

    private const string TestView1Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_1</c> view.
    /// </summary>
    public sealed record TestView1
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        public long? Testint { get; set; }

        /// <summary>
        /// The <c>testdouble</c> column.
        /// </summary>
        public double? Testdouble { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        public byte[]? Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        public string? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        public string? Teststring { get; set; }
    }
}";

    private const string TestView2Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_2</c> view.
    /// </summary>
    public sealed record TestView2
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        public long? Testint { get; set; }

        /// <summary>
        /// The <c>testdecimal</c> column.
        /// </summary>
        public decimal? Testdecimal { get; set; }

        /// <summary>
        /// The <c>testblob</c> column.
        /// </summary>
        public byte[]? Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        public decimal? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        public string? Teststring { get; set; }
    }
}";

    private readonly string TestView3Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// This is a test view comment for Poco
    /// </summary>
    public sealed record TestView3
    {
        /// <summary>
        /// This is a test column comment for Poco
        /// </summary>
        public long? TestColumn1 { get; set; }
    }
}";

    private readonly string TestView3MultiLineOutput = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test view comment for Poco.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    public sealed record TestView3
    {
        /// <summary>
        /// <para>This is a test column comment for Poco.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        public long? TestColumn1 { get; set; }
    }
}";
}