using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments
{
    internal sealed class SqlServerTableCommentProviderTests : SqlServerTest
    {
        private IRelationalDatabaseTableCommentProvider TableCommentProvider => new SqlServerTableCommentProvider(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create table table_comment_table_1 ( test_column_1 int )").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop table table_comment_table_1").ConfigureAwait(false);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresent_ReturnsTable()
        {
            var tableIsSome = await TableCommentProvider.GetTableComments("table_comment_table_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(tableIsSome);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresent_ReturnsTableWithCorrectName()
        {
            const string tableName = "table_comment_table_1";
            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, tableComments.TableName.LocalName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(tableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTable_WhenTablePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var tableName = new Identifier("A", "B", IdentifierDefaults.Schema, "table_comment_table_1");
            var expectedTableName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "table_comment_table_1");

            var tableComments = await TableCommentProvider.GetTableComments(tableName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedTableName, tableComments.TableName);
        }

        [Test]
        public async Task GetTableComments_WhenTableMissing_ReturnsNone()
        {
            var tableIsNone = await TableCommentProvider.GetTableComments("table_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(tableIsNone);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("TABLE_COMMENT_table_1");
            var tableComments = await TableCommentProvider.GetTableComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, tableComments.TableName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetTableComments_WhenTablePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "TABLE_COMMENT_table_1");
            var tableComments = await TableCommentProvider.GetTableComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, tableComments.TableName.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, tableComments.TableName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetAllTableComments_WhenEnumerated_ContainsTableComments()
        {
            var tableComments = await TableCommentProvider.GetAllTableComments().ConfigureAwait(false);

            Assert.NotZero(tableComments.Count);
        }

        [Test]
        public async Task GetAllTableComments_WhenEnumerated_ContainsTestTableComment()
        {
            var tables = await TableCommentProvider.GetAllTableComments().ConfigureAwait(false);
            var containsTestTable = tables.Any(t => t.TableName.LocalName == "table_comment_table_1");

            Assert.IsTrue(containsTestTable);
        }
    }
}
