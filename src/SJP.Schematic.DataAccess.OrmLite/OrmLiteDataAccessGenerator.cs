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

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteDataAccessGenerator : IDataAccessGenerator
    {
        public OrmLiteDataAccessGenerator(
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

        protected IFileSystem FileSystem { get; }

        protected IRelationalDatabase Database { get; }

        protected IRelationalDatabaseCommentProvider CommentProvider { get; }

        protected INameTranslator NameTranslator { get; }

        public Task Generate(string projectPath, string baseNamespace, CancellationToken cancellationToken = default)
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

            FileSystem.File.WriteAllText(projectPath, ProjectDefinition);

            var tableGenerator = new OrmLiteTableGenerator(NameTranslator, baseNamespace);
            var tables = await Database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var tableComments = await CommentProvider.GetAllTableComments(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var tableCommentsLookup = new Dictionary<Identifier, IRelationalDatabaseTableComments>();
            foreach (var comment in tableComments)
                tableCommentsLookup[comment.TableName] = comment;

            var viewGenerator = new OrmLiteViewGenerator(NameTranslator, baseNamespace);
            var views = await Database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var viewComments = await CommentProvider.GetAllViewComments(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
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

                FileSystem.File.WriteAllText(tablePath.FullName, tableClass);
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

                FileSystem.File.WriteAllText(viewPath.FullName, viewClass);
            }
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
                    new XElement("Nullable", "enable")
                ),
                new XElement(
                    "ItemGroup",
                    new XElement(
                        "PackageReference",
                        new XAttribute("Include", "ServiceStack.OrmLite"),
                        new XAttribute("Version", "5.8.0")
                    )
                )
            ).ToString(SaveOptions.None);
    }
}
