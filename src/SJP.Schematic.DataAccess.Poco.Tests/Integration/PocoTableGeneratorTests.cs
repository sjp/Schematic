﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.Poco.Tests.Integration
{
    internal sealed class PocoTableGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        private Task<IRelationalDatabaseTable> GetTable(Identifier tableName) => Database.GetTable(tableName).UnwrapSomeAsync();

        private static IDatabaseTableGenerator TableGenerator => new PocoTableGenerator(new PascalCaseNameTranslator(), TestNamespace);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"create table test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create table test_table_2 ( test_column_1 integer )").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table test_table_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table test_table_2").ConfigureAwait(false);
        }

        [Test]
        public async Task Generate_GivenTableWithVariousColumnTypes_GeneratesExpectedOutput()
        {
            var table = await GetTable("test_table_1").ConfigureAwait(false);
            var generator = TableGenerator;

            const string expected = TestTable1Output;
            var result = generator.Generate(table, Option<IRelationalDatabaseTableComments>.None);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = "This is a test table comment for Poco";
            const string columnComment = "This is a test column comment for Poco";

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
            var result = generator.Generate(table, comment);

            var expected = TestTable2Output;
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task Generate_GivenMultiLineTableWithTableAndColumnComments_GeneratesExpectedOutput()
        {
            const string tableComment = @"This is a test table comment for Poco.

This is a second line for it.";
            const string columnComment = @"This is a test column comment for Poco.

This is a second line for it.";

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
            var result = generator.Generate(table, comment);

            var expected = TestTable2MultiLineOutput;
            Assert.AreEqual(expected, result);
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
        public byte[] Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        public decimal? Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        public string Teststring { get; set; }
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
