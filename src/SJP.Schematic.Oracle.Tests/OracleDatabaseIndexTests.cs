using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseIndexTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.That(() => new OracleDatabaseIndex(null, isUnique, columns, properties), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, null, properties), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = Array.Empty<IDatabaseIndexColumn>();
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, columns, properties), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var columns = new IDatabaseIndexColumn[] { null };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, columns, properties), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidProperties_ThrowsArgumentException()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = (OracleIndexProperties)(-1);

            Assert.That(() => new OracleDatabaseIndex(indexName, isUnique, columns, properties), Throws.ArgumentException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.Name, Is.EqualTo(indexName));
        }

        [Test]
        public static void IsUnique_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.IsUnique, Is.EqualTo(isUnique));
        }

        [Test]
        public static void IsUnique_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = false;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.IsUnique, Is.EqualTo(isUnique));
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.Columns, Is.EqualTo(columns));
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithUniqueAndCreatedByConstraintPropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.Unique | OracleIndexProperties.CreatedByConstraint;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.GeneratedByConstraint, Is.True);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithoutUniqueAndCreatedByConstraintPropertyGet_ReturnsFalse()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.CreatedByConstraint;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.GeneratedByConstraint, Is.False);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithUniqueAndWithoutCreatedByConstraintPropertyGet_ReturnsFalse()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.Unique;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.GeneratedByConstraint, Is.False);
        }

        [Test]
        public static void GeneratedByConstraint_GivenPropertiesWithoutUniqueAndWithoutCreatedByConstraintPropertyGet_ReturnsFalse()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.Partitioned | OracleIndexProperties.Compressed;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.GeneratedByConstraint, Is.False);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            Identifier indexName = "test_index";
            const bool isUnique = true;
            var column = Mock.Of<IDatabaseIndexColumn>();
            var columns = new[] { column };
            const OracleIndexProperties properties = OracleIndexProperties.None;

            var index = new OracleDatabaseIndex(indexName, isUnique, columns, properties);

            Assert.That(index.IsEnabled, Is.True);
        }
    }
}
