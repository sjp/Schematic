﻿using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.Poco.Tests.Integration
{
    internal sealed class PocoViewGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection);

        private IRelationalDatabaseView GetView(Identifier viewName) => Database.GetView(viewName).UnwrapSome();

        private static IDatabaseViewGenerator ViewGenerator => new PocoViewGenerator(new PascalCaseNameProvider(), TestNamespace);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
create view test_view_1 as
select
    1 as testint,
    2.45 as testdouble,
    X'DEADBEEF' as testblob,
    CURRENT_TIMESTAMP as testdatetime,
    'asd' as teststring
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create table view_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'asd'
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view test_view_2 as select * from view_test_table_1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public void Generate_GivenViewWithLiteralColumnTypes_GeneratesExpectedOutput()
        {
            var view = GetView("test_view_1");
            var generator = ViewGenerator;

            const string expected = TestView1Output;
            var result = generator.Generate(view);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Generate_GivenViewSelectingFromTable_GeneratesExpectedOutput()
        {
            var view = GetView("test_view_2");
            var generator = ViewGenerator;

            const string expected = TestView2Output;
            var result = generator.Generate(view);

            Assert.AreEqual(expected, result);
        }

        private const string TestNamespace = "PocoTestNamespace";

        private const string TestView1Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_1</c> view.
    /// </summary>
    public class TestView1
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
        public byte[] Testblob { get; set; }

        /// <summary>
        /// The <c>testdatetime</c> column.
        /// </summary>
        public string Testdatetime { get; set; }

        /// <summary>
        /// The <c>teststring</c> column.
        /// </summary>
        public string Teststring { get; set; }
    }
}";

        private const string TestView2Output = @"using System;

namespace PocoTestNamespace.Main
{
    /// <summary>
    /// A mapping class to query the <c>test_view_2</c> view.
    /// </summary>
    public class TestView2
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
    }
}
