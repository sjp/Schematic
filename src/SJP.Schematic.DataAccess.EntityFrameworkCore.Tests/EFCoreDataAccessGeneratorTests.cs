using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal static class EFCoreDataAccessGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDataAccessGenerator(null, commentProvider, nameTranslator));
        }

        [Test]
        public static void Ctor_GivenNullCommentProvider_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDataAccessGenerator(database, null, nameTranslator));
        }

        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => new EFCoreDataAccessGenerator(database, commentProvider, null));
        }

        [Test]
        public static void Ctor_GivenNullIndent_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();

            Assert.Throws<ArgumentNullException>(() => new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator, null));
        }

        [Test]
        public static void Generate_GivenNullFileSystem_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null, projectPath, "test"));
        }

        [Test]
        public static void Generate_GivenNullProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, null, "test"));
        }

        [Test]
        public static void Generate_GivenEmptyProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, string.Empty, "test"));
        }

        [Test]
        public static void Generate_GivenWhiteSpaceProjectPath_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, "    ", "test"));
        }

        [Test]
        public static void Generate_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, projectPath, null));
        }

        [Test]
        public static void Generate_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, projectPath, string.Empty));
        }

        [Test]
        public static void Generate_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.csproj");

            Assert.Throws<ArgumentNullException>(() => generator.Generate(mockFs, projectPath, "    "));
        }

        [Test]
        public static void Generate_GivenProjectPathNotACsproj_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new EFCoreDataAccessGenerator(database, commentProvider, nameTranslator);
            var mockFs = new MockFileSystem();
            var projectPath = Path.Combine(Environment.CurrentDirectory, "DataAccessGeneratorTest.vbproj");

            Assert.Throws<ArgumentException>(() => generator.Generate(mockFs, projectPath, "test"));
        }
    }
}
