using System;
using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class DatabaseViewGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeDatabaseViewGenerator(null));
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseViewGenerator(nameProvider);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(null, "test"));
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseViewGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(baseDir, null));
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseViewGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Views", testViewName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, testViewName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseViewGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testViewSchema = "view_schema";
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Views", testViewSchema, testViewName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, new Identifier(testViewSchema, testViewName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }
    }
}
