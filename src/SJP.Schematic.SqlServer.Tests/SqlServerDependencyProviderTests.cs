using System;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerDependencyProviderTests
    {
        [Test]
        public void Ctor_GivenNullComparer_CreatesWithoutError()
        {
            var provider = new SqlServerDependencyProvider(null);
            Assert.Pass();
        }

        [Test]
        public void GetDependencies_GivenNullObjectName_ThrowsArgumentsNullException()
        {
            var provider = new SqlServerDependencyProvider();

            Assert.Throws<ArgumentNullException>(() => provider.GetDependencies(null, "asd"));
        }

        [Test]
        public void GetDependencies_GivenNullExpression_ThrowsArgumentsNullException()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";

            Assert.Throws<ArgumentNullException>(() => provider.GetDependencies(objectName, null));
        }

        [Test]
        public void GetDependencies_GivenEmptyExpression_ThrowsArgumentsNullException()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";

            Assert.Throws<ArgumentNullException>(() => provider.GetDependencies(objectName, string.Empty));
        }

        [Test]
        public void GetDependencies_GivenWhiteSpaceExpression_ThrowsArgumentsNullException()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";

            Assert.Throws<ArgumentNullException>(() => provider.GetDependencies(objectName, "    "));
        }

        [Test]
        public void GetDependencies_GivenExpressionWithSameObjectAsTable_ReturnsEmptyCollection()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = "select * from asd";

            var dependencies = provider.GetDependencies(objectName, expression);
            var count = dependencies.Count();

            Assert.Zero(count);
        }

        [Test]
        public void GetDependencies_GivenExpressionWithSameObjectAsFunction_ReturnsEmptyCollection()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = "select asd(1)";

            var dependencies = provider.GetDependencies(objectName, expression);
            var count = dependencies.Count();

            Assert.Zero(count);
        }

        [Test]
        public void GetDependencies_GivenExpressionPointingToOtherTable_ReturnsOtherTable()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = "select * from [other_table]";

            var dependencies = provider.GetDependencies(objectName, expression);
            var dependency = dependencies.Single();

            Assert.AreEqual("other_table", dependency.LocalName);
        }

        [Test]
        public void GetDependencies_GivenExpressionPointingToOtherFunction_ReturnsOtherFunction()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = "select other_function(1)";

            var dependencies = provider.GetDependencies(objectName, expression);
            var dependency = dependencies.Single();

            Assert.AreEqual("other_function", dependency.LocalName);
        }

        [Test]
        public void GetDependencies_GivenExpressionPointingToOtherColumns_ReturnsColumnNames()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = "([first_name] + ' ' + [last_name])";

            var dependencies = provider.GetDependencies(objectName, expression);
            var expectedNames = new[] { new Identifier("first_name"), new Identifier("last_name") };
            var equal = dependencies.SequenceEqual(expectedNames);

            Assert.IsTrue(equal);
        }

        [Test]
        public void GetDependencies_GivenExpressionForViewPointingToTableAndFunction_ReturnsColumnsTablesAndFunctions()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = @"
CREATE VIEW [asd] AS
SELECT 'zxc' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT * from dbo.FunctionName('testarg')
";

            var dependencies = provider.GetDependencies(objectName, expression);
            var expectedNames = new[]
            {
                new Identifier("FIRST_COL"),
                new Identifier("SECOND_COL"),
                new Identifier("FIRST_TABLE"),
                new Identifier("dbo", "FunctionName")
            };
            var equal = dependencies.SequenceEqual(expectedNames);

            Assert.IsTrue(equal);
        }

        [Test]
        public void GetDependencies_GivenExpressionForViewWithDuplicateNames_ReturnsUniqueDependencies()
        {
            var provider = new SqlServerDependencyProvider();
            Identifier objectName = "asd";
            const string expression = @"
CREATE VIEW [asd] AS
SELECT 'zxc' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT FIRST_COL, SECOND_COL from dbo.FunctionName('testarg')
";

            var dependencies = provider.GetDependencies(objectName, expression);
            var expectedNames = new[]
            {
                new Identifier("FIRST_COL"),
                new Identifier("SECOND_COL"),
                new Identifier("FIRST_TABLE"),
                new Identifier("dbo", "FunctionName")
            };
            var equal = dependencies.SequenceEqual(expectedNames);

            Assert.IsTrue(equal);
        }
    }
}
