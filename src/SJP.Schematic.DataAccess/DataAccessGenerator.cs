using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Common functionality for generating a data access project for a database.
/// </summary>
/// <seealso cref="IDataAccessGenerator" />
public abstract class DataAccessGenerator : IDataAccessGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataAccessGenerator"/> class.
    /// </summary>
    /// <param name="fileSystem">A file system to export to.</param>
    /// <param name="database">A relational database object provider.</param>
    /// <param name="commentProvider">A database comment provider.</param>
    /// <param name="nameTranslator">The name translator to use when generating C# object names.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="fileSystem"/>, <paramref name="database"/>, <paramref name="commentProvider"/>, <paramref name="nameTranslator"/> are <see langword="null" />.</exception>
    protected DataAccessGenerator(
        IFileSystem fileSystem,
        IRelationalDatabase database,
        IRelationalDatabaseCommentProvider commentProvider,
        INameTranslator nameTranslator)
    {
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        Database = database ?? throw new ArgumentNullException(nameof(database));
        CommentProvider = commentProvider ?? throw new ArgumentNullException(nameof(commentProvider));
        NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
    }

    /// <summary>
    /// The file system to export a project to.
    /// </summary>
    /// <value>A file system.</value>
    protected IFileSystem FileSystem { get; }

    /// <summary>
    /// A relational database that will be generated for.
    /// </summary>
    /// <value>A relational database.</value>
    protected IRelationalDatabase Database { get; }

    /// <summary>
    /// A comment provider for the given database.
    /// </summary>
    /// <value>A comment provider.</value>
    protected IRelationalDatabaseCommentProvider CommentProvider { get; }

    /// <summary>
    /// Gets the name translator.
    /// </summary>
    /// <value>The name translator.</value>
    protected INameTranslator NameTranslator { get; }

    /// <summary>
    /// The contents of the C# project file that the generated source files belong to.
    /// </summary>
    /// <value>An XML document describing a C# project.</value>
    protected abstract string ProjectDefinition { get; }

    /// <summary>
    /// Creates a generator that will be used to generate source code for each table in the database.
    /// </summary>
    /// <param name="baseNamespace">The base C# namespace to use for generated files.</param>
    /// <returns>A table generator.</returns>
    protected abstract IDatabaseTableGenerator CreateTableGenerator(string baseNamespace);

    /// <summary>
    /// Creates a generator that will be used to generate source code for each view in the database.
    /// </summary>
    /// <param name="baseNamespace">The base C# namespace to use for generated files.</param>
    /// <returns>A view generator.</returns>
    protected abstract IDatabaseViewGenerator CreateViewGenerator(string baseNamespace);

    /// <summary>
    /// Constructs the contents of a C# project file for a generated data access project.
    /// </summary>
    /// <param name="packageReferences">Any NuGet packages that the generated source code depends upon.</param>
    /// <returns>An XML document describing a C# project.</returns>
    protected static string BuildProjectDefinition(params (string PackageName, string Version)[] packageReferences)
    {
        ArgumentNullException.ThrowIfNull(packageReferences);

        var project = new XElement(
            "Project",
            new XAttribute("Sdk", "Microsoft.NET.Sdk"),
            new XElement(
                "PropertyGroup",
                new XElement("TargetFramework", "net10.0"),
                new XElement("CheckForOverflowUnderflow", true),
                new XElement("TreatWarningsAsErrors", true),
                new XElement("Nullable", "enable"),
                new XElement("LangVersion", "latest"),
                new XElement("Features", "strict"),
                new XElement("AnalysisLevel", "latest")
            )
        );

        if (packageReferences.Length > 0)
        {
            project.Add(
                new XElement(
                    "ItemGroup",
                    packageReferences.Select(static p => new XElement(
                        "PackageReference",
                        new XAttribute("Include", p.PackageName),
                        new XAttribute("Version", p.Version)))));
        }

        return project.ToString(SaveOptions.None);
    }

    /// <summary>
    /// Generates any source files required by the project in addition to the table and view classes.
    /// </summary>
    /// <param name="projectDirectory">The directory that the project is generated in.</param>
    /// <param name="baseNamespace">The base C# namespace to use for generated files.</param>
    /// <param name="tables">The tables in the database.</param>
    /// <param name="views">The views in the database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The paths of any files that were generated. Empty when no further files are required.</returns>
    protected virtual Task<IEnumerable<string>> GenerateAdditionalFilesAsync(
        IDirectoryInfo projectDirectory,
        string baseNamespace,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<string>());

    /// <summary>
    /// Generates a data access project in C#.
    /// </summary>
    /// <param name="projectPath">A path that determines where the generated C# project file should be stored.</param>
    /// <param name="baseNamespace">The base C# namespace to use for generated files.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task indicating the completion of the source code generation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="projectPath"/> or <paramref name="baseNamespace"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="projectPath"/> or <paramref name="baseNamespace"/> is empty or whitespace, or <paramref name="projectPath"/> is not a path to a <c>csproj</c> file.</exception>
    public Task GenerateAsync(string projectPath, string baseNamespace, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseNamespace);

        var projectFileInfo = FileSystem.FileInfo.New(projectPath);
        if (!string.Equals(projectFileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

        return GenerateAsyncCore(projectFileInfo, baseNamespace, cancellationToken);
    }

    private async Task GenerateAsyncCore(IFileInfo projectFileInfo, string baseNamespace, CancellationToken cancellationToken)
    {
        // A project file always lives inside a directory, only a root path (which cannot be a csproj) has none.
        var projectDirectory = projectFileInfo.Directory!;

        await WriteFileAsync(projectFileInfo, ProjectDefinition, cancellationToken);

        var (
            tables,
            tableComments,
            views,
            viewComments
        ) = await (
            Database.GetAllTables(cancellationToken),
            CommentProvider.GetAllTableComments(cancellationToken),
            Database.GetAllViews(cancellationToken),
            CommentProvider.GetAllViewComments(cancellationToken)
        ).WhenAll();

        var tableCommentsLookup = new Dictionary<Identifier, IRelationalDatabaseTableComments>();
        foreach (var comment in tableComments)
            tableCommentsLookup[comment.TableName] = comment;

        var viewCommentsLookup = new Dictionary<Identifier, IDatabaseViewComments>();
        foreach (var comment in viewComments)
            viewCommentsLookup[comment.ViewName] = comment;

        var generatedFilePaths = new List<string>();

        var tableGenerator = CreateTableGenerator(baseNamespace);
        foreach (var table in tables)
        {
            var tableComment = tableCommentsLookup.TryGetValue(table.Name, out var comment)
                ? Option<IRelationalDatabaseTableComments>.Some(comment)
                : Option<IRelationalDatabaseTableComments>.None;

            var tableClass = tableGenerator.Generate(tables, table, tableComment);
            var tablePath = tableGenerator.GetFilePath(projectDirectory, table.Name);

            await WriteFileAsync(tablePath, tableClass, cancellationToken);
            generatedFilePaths.Add(tablePath.FullName);
        }

        var viewGenerator = CreateViewGenerator(baseNamespace);
        foreach (var view in views)
        {
            var viewComment = viewCommentsLookup.TryGetValue(view.Name, out var comment)
                ? Option<IDatabaseViewComments>.Some(comment)
                : Option<IDatabaseViewComments>.None;

            var viewClass = viewGenerator.Generate(view, viewComment);
            var viewPath = viewGenerator.GetFilePath(projectDirectory, view.Name);

            await WriteFileAsync(viewPath, viewClass, cancellationToken);
            generatedFilePaths.Add(viewPath.FullName);
        }

        var additionalFilePaths = await GenerateAdditionalFilesAsync(projectDirectory, baseNamespace, tables, views, cancellationToken);
        generatedFilePaths.AddRange(additionalFilePaths);

        ProjectFileCleaner.RemoveStaleFiles(projectDirectory, generatedFilePaths);
    }

    private async Task WriteFileAsync(IFileInfo file, string contents, CancellationToken cancellationToken)
    {
        var directory = file.Directory;
        if (directory != null && !directory.Exists)
            directory.Create();

        await FileSystem.File.WriteAllTextAsync(file.FullName, contents, cancellationToken);
    }
}
