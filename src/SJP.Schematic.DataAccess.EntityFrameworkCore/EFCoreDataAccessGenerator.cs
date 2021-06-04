using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    /// <summary>
    /// A data access project generator for Entity Framework Core.
    /// </summary>
    /// <seealso cref="IDataAccessGenerator" />
    public class EFCoreDataAccessGenerator : IDataAccessGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EFCoreDataAccessGenerator"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system to export to.</param>
        /// <param name="database">A relational database object provider.</param>
        /// <param name="commentProvider">A database comment provider.</param>
        /// <param name="nameTranslator">The name translator to use when generating C# object names.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="fileSystem"/>, <paramref name="database"/>, <paramref name="commentProvider"/>, <paramref name="nameTranslator"/> are <c>null</c>.</exception>
        public EFCoreDataAccessGenerator(
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
        /// Generates an Entity Framework Core project.
        /// </summary>
        /// <param name="projectPath">A path that determines where the generated C# project file should be stored.</param>
        /// <param name="baseNamespace">The base C# namespace to use for generated files.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task indicating the completion of the source code generation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="projectPath"/> or <paramref name="baseNamespace"/> are <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="projectPath"/> is not a path to a <c>csproj</c> file.</exception>
        public Task GenerateAsync(string projectPath, string baseNamespace, CancellationToken cancellationToken = default)
        {
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectFileInfo = FileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectFileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            return GenerateAsyncCore(projectPath, baseNamespace, cancellationToken);
        }

        private async Task GenerateAsyncCore(string projectPath, string baseNamespace, CancellationToken cancellationToken)
        {
            var projectFileInfo = FileSystem.FileInfo.FromFileName(projectPath);
            if (projectFileInfo.Exists)
                projectFileInfo.Delete();

            if (!projectFileInfo.Directory.Exists)
                projectFileInfo.Directory.Create();

            await FileSystem.File.WriteAllTextAsync(projectPath, ProjectDefinition, cancellationToken).ConfigureAwait(false);

            var dbContextGenerator = new EFCoreDbContextBuilder(NameTranslator, baseNamespace);
            var tableGenerator = new EFCoreTableGenerator(FileSystem, NameTranslator, baseNamespace);
            var viewGenerator = new EFCoreViewGenerator(FileSystem, NameTranslator, baseNamespace);

            var tables = await Database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var tableComments = await CommentProvider.GetAllTableComments(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var views = await Database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var viewComments = await CommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var sequences = await Database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

            var tableCommentsLookup = new Dictionary<Identifier, IRelationalDatabaseTableComments>();
            foreach (var comment in tableComments)
                tableCommentsLookup[comment.TableName] = comment;

            var viewCommentsLookup = new Dictionary<Identifier, IDatabaseViewComments>();
            foreach (var comment in viewComments)
                viewCommentsLookup[comment.ViewName] = comment;

            foreach (var table in tables)
            {
                var tableComment = tableCommentsLookup.ContainsKey(table.Name)
                    ? Option<IRelationalDatabaseTableComments>.Some(tableCommentsLookup[table.Name])
                    : Option<IRelationalDatabaseTableComments>.None;

                var tableClass = tableGenerator.Generate(tables, table, tableComment);
                var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                await FileSystem.File.WriteAllTextAsync(tablePath.FullName, tableClass, cancellationToken).ConfigureAwait(false);
            }

            foreach (var view in views)
            {
                var viewComment = viewCommentsLookup.ContainsKey(view.Name)
                    ? Option<IDatabaseViewComments>.Some(viewCommentsLookup[view.Name])
                    : Option<IDatabaseViewComments>.None;

                var viewClass = viewGenerator.Generate(view, viewComment);
                var viewPath = viewGenerator.GetFilePath(projectFileInfo.Directory, view.Name);

                if (!viewPath.Directory.Exists)
                    viewPath.Directory.Create();

                if (viewPath.Exists)
                    viewPath.Delete();

                await FileSystem.File.WriteAllTextAsync(viewPath.FullName, viewClass, cancellationToken).ConfigureAwait(false);
            }

            var dbContextText = dbContextGenerator.Generate(tables, views, sequences);
            var dbContextPath = Path.Combine(projectFileInfo.Directory.FullName, "AppContext.cs");

            await FileSystem.File.WriteAllTextAsync(dbContextPath, dbContextText, cancellationToken).ConfigureAwait(false);
        }

        private static string ProjectDefinition { get; } =
            new XElement(
                "Project",
                new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                new XElement(
                    "PropertyGroup",
                    new XElement("TargetFramework", "netstandard2.1"),
                    new XElement("CheckForOverflowUnderflow", true),
                    new XElement("TreatWarningsAsErrors", true),
                    new XElement("Nullable", "enable"),
                    new XElement("LangVersion", "latest"),
                    new XElement("Features", "strict"),
                    new XElement("AnalysisLevel", "latest")
                ),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "Microsoft.EntityFrameworkCore.Relational"),
                        new XAttribute("Version", GetEfCoreVersionString())
                    )
                )
            ).ToString(SaveOptions.None);

        private static string GetEfCoreVersionString()
        {
            var efCoreAssembly = typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly;
            return FileVersionInfo.GetVersionInfo(efCoreAssembly.Location).ProductVersion ?? string.Empty;
        }
    }
}
