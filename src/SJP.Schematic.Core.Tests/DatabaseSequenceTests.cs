using System;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal class DatabaseSequenceTests
    {
        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSequence(null, "test", 1, 1, null, null, true, 0));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseSequence(database, null, 1, 1, null, null, true, 0));
        }

        [Test]
        public void Ctor_GivenZeroIncrement_ThrowsArgException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new DatabaseSequence(database, "test", 1, 0, null, null, true, 0));
        }

        [Test]
        public void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequence = new DatabaseSequence(database, "test", 1, 1, null, null, true, 0);

            Assert.AreSame(database, sequence.Database);
        }

        [Test]
        public void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            var sequenceName = new Identifier("test");
            var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "test");

            var sequence = new DatabaseSequence(database, "test", 1, 1, null, null, true, 0);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            var database = databaseMock.Object;

            var sequenceName = new Identifier("c", "d");
            var expectedSequenceName = new Identifier(database.ServerName, database.DatabaseName, "c", "d");

            var sequence = new DatabaseSequence(database, sequenceName, 1, 1, null, null, true, 0);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            var database = databaseMock.Object;

            var sequenceName = new Identifier("qwe", "asd", "sequence_test_sequence_1");
            var expectedSequenceName = new Identifier(database.ServerName, "qwe", "asd", "sequence_test_sequence_1");

            var sequence = new DatabaseSequence(database, sequenceName, 1, 1, null, null, true, 0);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequenceName = new Identifier("qwe", "asd", "zxc", "sequence_test_sequence_1");
            var expectedSequenceName = new Identifier("qwe", "asd", "zxc", "sequence_test_sequence_1");

            var sequence = new DatabaseSequence(database, sequenceName, 1, 1, null, null, true, 0);

            Assert.AreEqual(expectedSequenceName, sequence.Name);
        }

        [Test]
        public void Ctor_GivenPositiveIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new DatabaseSequence(database, "test", 1, 1, 2, null, true, 0));
        }

        [Test]
        public void Ctor_GivenPositiveIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new DatabaseSequence(database, "test", 1, 1, null, -1, true, 0));
        }

        [Test]
        public void Ctor_GivenNegativeIncrementAndMinValueLessThanStart_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new DatabaseSequence(database, "test", 1, -1, 0, null, true, 0));
        }

        [Test]
        public void Ctor_GivenNegativeIncrementAndMaxValueGreaterThanStart_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new DatabaseSequence(database, "test", 1, -1, null, 2, true, 0));
        }

        [Test]
        public void Ctor_GivenNegativeCacheSize_SetsCacheSizeToUnknownValue()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequence =  new DatabaseSequence(database, "test", 1, -1, null, null, true, -3);

            Assert.AreEqual(DatabaseSequence.UnknownCacheSize, sequence.Cache);
        }

        [Test]
        public void Cache_PropertyGet_MatchesCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const int cacheSize = 20;
            var sequence = new DatabaseSequence(database, "test", 1, 1, null, null, true, cacheSize);

            Assert.AreEqual(cacheSize, sequence.Cache);
        }

        [Test]
        public void Cycle_PropertyGet_MatchesCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var sequence = new DatabaseSequence(database, "test", 1, 1, null, null, true, 1);

            Assert.IsTrue(sequence.Cycle);
        }

        [Test]
        public void Increment_PropertyGet_MatchesCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const int increment = 100;
            var sequence = new DatabaseSequence(database, "test", 1, increment, null, null, true, 1);

            Assert.AreEqual(increment, sequence.Increment);
        }

        [Test]
        public void MaxValue_PropertyGet_MatchesCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const int maxValue = 100;
            var sequence = new DatabaseSequence(database, "test", 1, 1, null, maxValue, true, 1);

            Assert.AreEqual(maxValue, sequence.MaxValue);
        }

        [Test]
        public void MinValue_PropertyGet_MatchesCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const int minValue = 100;
            var sequence = new DatabaseSequence(database, "test", minValue, 1, minValue, null, true, 1);

            Assert.AreEqual(minValue, sequence.MinValue);
        }

        [Test]
        public void Start_PropertyGet_MatchesCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const int start = 100;
            var sequence = new DatabaseSequence(database, "test", start, 1, null, null, true, 1);

            Assert.AreEqual(start, sequence.Start);
        }
    }
}
