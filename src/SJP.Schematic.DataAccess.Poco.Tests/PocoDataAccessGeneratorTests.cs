using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.Poco.Tests;

[TestFixture]
internal static class PocoDataAccessGeneratorTests
{
    private const string TestCsprojFileName = "DataAccessGeneratorTest.csproj";

    private static IIdentifierDefaults IdentifierDefaults => new IdentifierDefaults("a", "b", "c");

    [Test]
    public static void Ctor_GivenNullFileSystem_ThrowsArgumentNullException()
    {
        var database = Mock.Of<IRelationalDatabase>();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new PocoDataAccessGenerator(null, database, commentProvider, nameTranslator), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
    {
        var mockFs = new MockFileSystem();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new PocoDataAccessGenerator(mockFs, null, commentProvider, nameTranslator), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullCommentProvider_ThrowsArgumentNullException()
    {
        var mockFs = new MockFileSystem();
        var database = Mock.Of<IRelationalDatabase>();
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new PocoDataAccessGenerator(mockFs, database, null, nameTranslator), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
    {
        var mockFs = new MockFileSystem();
        var database = Mock.Of<IRelationalDatabase>();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        Assert.That(() => new PocoDataAccessGenerator(mockFs, database, commentProvider, null), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void GenerateAsync_GivenNullProjectPath_ThrowsArgumentException(string projectPath)
    {
        var mockFs = new MockFileSystem();
        var database = Mock.Of<IRelationalDatabase>();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new VerbatimNameTranslator();
        var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);

        Assert.That(() => generator.GenerateAsync(projectPath, "test"), Throws.InstanceOf<ArgumentException>());
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void GenerateAsync_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentException(string ns)
    {
        var mockFs = new MockFileSystem();
        var database = Mock.Of<IRelationalDatabase>();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new VerbatimNameTranslator();
        var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
        using var tempDir = new TemporaryDirectory();
        var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFileName);

        Assert.That(() => generator.GenerateAsync(projectPath, ns), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void GenerateAsync_GivenProjectPathNotACsproj_ThrowsArgumentException()
    {
        var mockFs = new MockFileSystem();
        var database = Mock.Of<IRelationalDatabase>();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new VerbatimNameTranslator();
        var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
        using var tempDir = new TemporaryDirectory();
        var projectPath = Path.Combine(tempDir.DirectoryPath, "DataAccessGeneratorTest.vbproj");

        Assert.That(() => generator.GenerateAsync(projectPath, "test"), Throws.ArgumentException);
    }
}