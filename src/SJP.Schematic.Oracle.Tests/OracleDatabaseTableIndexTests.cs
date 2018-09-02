using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseTableIndexTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseTableIndex(null, indexName, isUnique, columns, properties));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseTableIndex(table, null, isUnique, columns, properties));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseTableIndex(table, indexName, isUnique, null, properties));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties));
        }

        [Test]
        public static void Ctor_GivenInvalidProperties_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = (OracleIndexProperties)(-1);

            Assert.Throws<ArgumentException>(() => new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.AreEqual(table, index.Table);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.AreEqual(indexName, index.Name);
        }

        [Test]
        public static void IsUnique_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.AreEqual(isUnique, index.IsUnique);
        }

        [Test]
        public static void IsUnique_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.AreEqual(isUnique, index.IsUnique);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.AreEqual(columns, index.Columns);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithUniqueAndCreatedByConstraintPropertyGet_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.Unique | OracleIndexProperties.CreatedByConstraint;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.IsTrue(index.GeneratedByConstraint);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithoutUniqueAndCreatedByConstraintPropertyGet_ReturnsFalse()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.CreatedByConstraint;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.IsFalse(index.GeneratedByConstraint);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithUniqueAndWithoutCreatedByConstraintPropertyGet_ReturnsFalse()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.Unique;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.IsFalse(index.GeneratedByConstraint);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithoutUniqueAndWithoutCreatedByConstraintPropertyGet_ReturnsFalse()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.Partitioned | OracleIndexProperties.Compressed;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.IsFalse(index.GeneratedByConstraint);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseTableIndex(table, indexName, isUnique, columns, properties);

            Assert.IsTrue(index.IsEnabled);
        }
    }
}
