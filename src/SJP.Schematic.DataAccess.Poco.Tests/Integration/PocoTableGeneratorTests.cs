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

namespace SJP.Schematic.DataAccess.Poco.Tests.Integration
{
    internal sealed class PocoTableGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private static IDatabaseTableGenerator TableGenerator => new PocoTableGenerator(new MockFileSystem(), new PascalCaseNameTranslator(), TestNamespace);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync(@"create table test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("create table test_table_2 ( test_column_1 integer )", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop table test_table_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop table test_table_2", CancellationToken.None).ConfigureAwait(false);
        }

        [Test]
        public async Task Generate_GivenTableWithVariousColumnTypes_GeneratesExpectedOutput()
        {
            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_1").ConfigureAwait(false);
            var generator = TableGenerator;

            const string expected = TestTable1Output;
            var result = generator.Generate(tables, table, Option<IRelationalDatabaseTableComments>.None);

            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for Poco";
            const string columnComment = "This is a test column comment for Poco";

            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_2").ConfigureAwait(false);
            var generator = TableGenerator;

            var comment = new RelationalDatabaseTableComments("test_table_2",
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

            var expected = TestTable2Output;
            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for Poco.

This is a second line for it.";
            const string columnComment = @"This is a test column comment for Poco.

This is a second line for it.";

            var tables = await Database.GetAllTables().ToListAsync().ConfigureAwait(false);
            var table = await GetTable("test_table_2").ConfigureAwait(false);
            var generator = TableGenerator;

            var comment = new RelationalDatabaseTableComments("test_table_2",
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

            var expected = TestTable2MultiLineOutput;
            Assert.That(result, Is.EqualTo(expected).Using(LineEndingInvariantStringComparer.Ordinal));
        }

        private const string TestNamespace = "PocoTestNamespace";

        private const string TestTable1Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_table_1</c> table.
    /// </summary>
    public class TestTable1
    {
        /// <summary>
        /// The <c>testint</c> column.
        /// </summary>
        public long Testint { get; set; }

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

        private readonly string TestTable2Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// This is a test table comment for Poco
    /// </summary>
    public class TestTable2
    {
        /// <summary>
        /// This is a test column comment for Poco
        /// </summary>
        public long? TestColumn1 { get; set; }
    }
}";

        private readonly string TestTable2MultiLineOutput = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// <para>This is a test table comment for Poco.</para>
    /// <para>This is a second line for it.</para>
    /// </summary>
    public class TestTable2
    {
        /// <summary>
        /// <para>This is a test column comment for Poco.</para>
        /// <para>This is a second line for it.</para>
        /// </summary>
        public long? TestColumn1 { get; set; }
    }
}";
    }
}
