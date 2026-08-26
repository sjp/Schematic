using System;
using System.Diagnostics;
using System.IO.Abstractions;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.OrmLite;

/// <summary>
/// A data access project generator for OrmLite.
/// </summary>
/// <seealso cref="DataAccessGenerator" />
public class OrmLiteDataAccessGenerator : DataAccessGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrmLiteDataAccessGenerator"/> class.
    /// </summary>
    /// <param name="fileSystem">A file system to export to.</param>
    /// <param name="database">A relational database object provider.</param>
    /// <param name="commentProvider">A database comment provider.</param>
    /// <param name="nameTranslator">The name translator to use when generating C# object names.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="fileSystem"/>, <paramref name="database"/>, <paramref name="commentProvider"/>, <paramref name="nameTranslator"/> are <see langword="null" />.</exception>
    public OrmLiteDataAccessGenerator(
        IFileSystem fileSystem,
        IRelationalDatabase database,
        IRelationalDatabaseCommentProvider commentProvider,
        INameTranslator nameTranslator)
        : base(fileSystem, database, commentProvider, nameTranslator)
    {
    }

    /// <inheritdoc />
    protected override IDatabaseTableGenerator CreateTableGenerator(string baseNamespace) => new OrmLiteTableGenerator(FileSystem, NameTranslator, baseNamespace);

    /// <inheritdoc />
    protected override IDatabaseViewGenerator CreateViewGenerator(string baseNamespace) => new OrmLiteViewGenerator(FileSystem, NameTranslator, baseNamespace);

    /// <inheritdoc />
    protected override string ProjectDefinition => ProjectDefinitionXml;

    private static readonly string ProjectDefinitionXml = BuildProjectDefinition(("ServiceStack.OrmLite", GetOrmLiteVersionString()));

    private static string GetOrmLiteVersionString()
    {
        var ormliteAssembly = typeof(ServiceStack.OrmLite.OrmLiteConfig).Assembly;
        var productVersion = FileVersionInfo.GetVersionInfo(ormliteAssembly.Location).ProductVersion ?? string.Empty;

        return productVersion.Split('+', 2, StringSplitOptions.RemoveEmptyEntries)[0];
    }
}
