using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal class EFCoreDataAccessGeneratorTests
    {
        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDataAccessGenerator(null, nameProvider));
        }

        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDataAccessGenerator(database, null));
        }

        [Test]
        public void Generate_GivenNullFileSystem_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null, projectPath, "testns"));
        }

        [Test]
        public void Generate_GivenNullProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, null, "testns"));
        }

        [Test]
        public void Generate_GivenEmptyProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, string.Empty, "testns"));
        }

        [Test]
        public void Generate_GivenWhiteSpaceProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, "    ", "testns"));
        }

        [Test]
        public void Generate_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, projectPath, null));
        }

        [Test]
        public void Generate_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, projectPath, string.Empty));
        }

        [Test]
        public void Generate_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, projectPath, "    "));
        }

        [Test]
        public void Generate_GivenProjectPathNotACsproj_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameProvider = new VerbatimNameProvider();
            var generator = new EFCoreDataAccessGenerator(database, nameProvider);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.vbproj");

            Assert.Throws<ArgumentException>(() => generator.Generate(mockFs, projectPath, "testns"));
        }
    }
}
