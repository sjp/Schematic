using System;
using System.IO;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Poco.Tests
{
    [TestFixture]
    public class DataAccessGeneratorTests
    {
        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new DataAccessGenerator(null, nameProvider));
        }

        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new DataAccessGenerator(database, null));
        }

        [Test]
        public void Generate_GivenNullProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new DataAccessGenerator(database, nameProvider);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null, "testns"));
        }

        [Test]
        public void Generate_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new DataAccessGenerator(database, nameProvider);
            var projectPath = new FileInfo(Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj"));

            Assert.Throws<ArgumentNullException>(() => generator.Generate(projectPath, null));
        }

        [Test]
        public void Generate_GivenEmptyNamespace_ThrowsArgumentNullException()
        {

            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new DataAccessGenerator(database, nameProvider);
            var projectPath = new FileInfo(Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj"));

            Assert.Throws<ArgumentNullException>(() => generator.Generate(projectPath, string.Empty));
        }

        [Test]
        public void Generate_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {

            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new DataAccessGenerator(database, nameProvider);
            var projectPath = new FileInfo(Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj"));

            Assert.Throws<ArgumentNullException>(() => generator.Generate(projectPath, "    "));
        }

        [Test]
        public void Generate_GivenProjectPathNotACsproj_ThrowsArgumentException()
        {

            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new DataAccessGenerator(database, nameProvider);
            var projectPath = new FileInfo(Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.vbproj"));

            Assert.Throws<ArgumentException>(() => generator.Generate(projectPath, "testns"));
        }
    }
}
