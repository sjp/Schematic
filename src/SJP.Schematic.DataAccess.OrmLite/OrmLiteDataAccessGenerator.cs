using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteDataAccessGenerator : IDataAccessGenerator
    {
        public OrmLiteDataAccessGenerator(
            IRelationalDatabase database,
            IRelationalDatabaseCommentProvider commentProvider,
            INameTranslator nameTranslator,
            string indent = "    ")
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            CommentProvider = commentProvider ?? throw new ArgumentNullException(nameof(commentProvider));
            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Indent = indent ?? throw new ArgumentNullException(nameof(indent));
        }

        protected IRelationalDatabase Database { get; }

        protected IRelationalDatabaseCommentProvider CommentProvider { get; }

        protected INameTranslator NameTranslator { get; }

        protected string Indent { get; }

        public void Generate(IFileSystem fileSystem, string projectPath, string baseNamespace)
        {
            if (fileSystem == null)
                throw new ArgumentNullException(nameof(fileSystem));
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectFileInfo = fileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectFileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectFileInfo.Exists)
                projectFileInfo.Delete();

            if (!projectFileInfo.Directory.Exists)
                projectFileInfo.Directory.Create();

            fileSystem.File.WriteAllText(projectPath, ProjectDefinition);

            var tableGenerator = new OrmLiteTableGenerator(NameTranslator, baseNamespace, Indent);
            var viewGenerator = new OrmLiteViewGenerator(NameTranslator, baseNamespace, Indent);

            var tables = Database.GetAllTables(CancellationToken.None).GetAwaiter().GetResult();
            var tableComments = CommentProvider.GetAllTableComments(CancellationToken.None).GetAwaiter().GetResult();

            var tableCommentsLookup = new Dictionary<Identifier, IRelationalDatabaseTableComments>();
            foreach (var comment in tableComments)
                tableCommentsLookup[comment.TableName] = comment;

            foreach (var table in tables)
            {
                var tableComment = tableCommentsLookup.ContainsKey(table.Name)
                    ? Option<IRelationalDatabaseTableComments>.Some(tableCommentsLookup[table.Name])
                    : Option<IRelationalDatabaseTableComments>.None;

                var tableClass = tableGenerator.Generate(table, tableComment);
                var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                fileSystem.File.WriteAllText(tablePath.FullName, tableClass);
            }

            var views = Database.GetAllViews(CancellationToken.None).GetAwaiter().GetResult();
            var viewComments = CommentProvider.GetAllViewComments(CancellationToken.None).GetAwaiter().GetResult();

            var viewCommentsLookup = new Dictionary<Identifier, IDatabaseViewComments>();
            foreach (var comment in viewComments)
                viewCommentsLookup[comment.ViewName] = comment;

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

                fileSystem.File.WriteAllText(viewPath.FullName, viewClass);
            }
        }

        protected const string ProjectDefinition = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""ServiceStack.OrmLite"" Version=""5.4.0"" />
    </ItemGroup>
</Project>";
    }
}
