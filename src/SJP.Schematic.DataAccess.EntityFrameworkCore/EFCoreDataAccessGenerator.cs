using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore;

/// <summary>
/// A data access project generator for Entity Framework Core.
/// </summary>
/// <seealso cref="DataAccessGenerator" />
public class EFCoreDataAccessGenerator : DataAccessGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreDataAccessGenerator"/> class.
    /// </summary>
    /// <param name="fileSystem">A file system to export to.</param>
    /// <param name="database">A relational database object provider.</param>
    /// <param name="commentProvider">A database comment provider.</param>
    /// <param name="nameTranslator">The name translator to use when generating C# object names.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="fileSystem"/>, <paramref name="database"/>, <paramref name="commentProvider"/>, <paramref name="nameTranslator"/> are <see langword="null" />.</exception>
    public EFCoreDataAccessGenerator(
        IFileSystem fileSystem,
        IRelationalDatabase database,
        IRelationalDatabaseCommentProvider commentProvider,
        INameTranslator nameTranslator)
        : base(fileSystem, database, commentProvider, nameTranslator)
    {
    }

    /// <inheritdoc />
    protected override IDatabaseTableGenerator CreateTableGenerator(string baseNamespace) => new EFCoreTableGenerator(FileSystem, NameTranslator, baseNamespace);

    /// <inheritdoc />
    protected override IDatabaseViewGenerator CreateViewGenerator(string baseNamespace) => new EFCoreViewGenerator(FileSystem, NameTranslator, baseNamespace);

    /// <inheritdoc />
    /// <remarks>Also generates the <c>DbContext</c> that exposes the generated table and view classes.</remarks>
    protected override async Task<IEnumerable<string>> GenerateAdditionalFilesAsync(
        IDirectoryInfo projectDirectory,
        string baseNamespace,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectDirectory);

        var sequences = await Database.GetAllSequences(cancellationToken);

        var dbContextGenerator = new EFCoreDbContextBuilder(NameTranslator, baseNamespace);
        var dbContextText = dbContextGenerator.Generate(tables, views, sequences);
        var dbContextPath = FileSystem.Path.Combine(projectDirectory.FullName, "AppContext.cs");

        await FileSystem.File.WriteAllTextAsync(dbContextPath, dbContextText, cancellationToken);

        return [dbContextPath];
    }

    /// <inheritdoc />
    protected override string ProjectDefinition => ProjectDefinitionXml;

    private static readonly string ProjectDefinitionXml = BuildProjectDefinition(("Microsoft.EntityFrameworkCore.Relational", GetEfCoreVersionString()));

    private static string GetEfCoreVersionString()
    {
        var efCoreAssembly = typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly;
        return FileVersionInfo.GetVersionInfo(efCoreAssembly.Location).ProductVersion ?? string.Empty;
    }
}
