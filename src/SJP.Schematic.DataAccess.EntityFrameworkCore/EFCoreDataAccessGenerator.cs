using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class EFCoreDataAccessGenerator : IDataAccessGenerator
    {
        public EFCoreDataAccessGenerator(
            IFileSystem fileSystem,
            IRelationalDatabase database,
            IRelationalDatabaseCommentProvider commentProvider,
            INameTranslator nameTranslator,
            string indent = "    ")
        {
            FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            CommentProvider = commentProvider ?? throw new ArgumentNullException(nameof(commentProvider));
            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Indent = indent ?? throw new ArgumentNullException(nameof(indent));
        }

        protected IFileSystem FileSystem { get; }

        protected IRelationalDatabase Database { get; }

        protected IRelationalDatabaseCommentProvider CommentProvider { get; }

        protected INameTranslator NameTranslator { get; }

        protected string Indent { get; }

        public void Generate(string projectPath, string baseNamespace)
        {
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectFileInfo = FileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectFileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            var tables = Database.GetAllTables(CancellationToken.None).GetAwaiter().GetResult();
            var sequences = Database.GetAllSequences(CancellationToken.None).GetAwaiter().GetResult();
            var comments = CommentProvider.GetAllTableComments(CancellationToken.None).GetAwaiter().GetResult();

            if (projectFileInfo.Exists)
                projectFileInfo.Delete();

            if (!projectFileInfo.Directory.Exists)
                projectFileInfo.Directory.Create();

            FileSystem.File.WriteAllText(projectPath, ProjectDefinition);

            var dbContextGenerator = new EFCoreDbContextBuilder(NameTranslator, baseNamespace);
            var tableGenerator = new EFCoreTableGenerator(NameTranslator, baseNamespace, Indent);

            var commentsLookup = new Dictionary<Identifier, IRelationalDatabaseTableComments>();
            foreach (var comment in comments)
                commentsLookup[comment.TableName] = comment;

            foreach (var table in tables)
            {
                var tableComment = commentsLookup.ContainsKey(table.Name)
                    ? Option<IRelationalDatabaseTableComments>.Some(commentsLookup[table.Name])
                    : Option<IRelationalDatabaseTableComments>.None;

                var tableClass = tableGenerator.Generate(table, tableComment);
                var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                FileSystem.File.WriteAllText(tablePath.FullName, tableClass);
            }

            var dbContextText = dbContextGenerator.Generate(tables, sequences);
            var dbContextPath = Path.Combine(projectFileInfo.Directory.FullName, "AppContext.cs");

            FileSystem.File.WriteAllText(dbContextPath, dbContextText);
        }

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

            FileSystem.File.WriteAllText(projectPath, ProjectDefinition);

            var dbContextGenerator = new EFCoreDbContextBuilder(NameTranslator, baseNamespace);
            var tableGenerator = new EFCoreTableGenerator(NameTranslator, baseNamespace, Indent);

            var tables = await Database.GetAllTables(cancellationToken).ConfigureAwait(false);
            var sequences = await Database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            var comments = await CommentProvider.GetAllTableComments(cancellationToken).ConfigureAwait(false);

            var commentsLookup = new Dictionary<Identifier, IRelationalDatabaseTableComments>();
            foreach (var comment in comments)
                commentsLookup[comment.TableName] = comment;

            foreach (var table in tables)
            {
                var tableComment = commentsLookup.ContainsKey(table.Name)
                    ? Option<IRelationalDatabaseTableComments>.Some(commentsLookup[table.Name])
                    : Option<IRelationalDatabaseTableComments>.None;

                var tableClass = tableGenerator.Generate(table, tableComment);
                var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                FileSystem.File.WriteAllText(tablePath.FullName, tableClass);
            }

            var dbContextText = dbContextGenerator.Generate(tables, sequences);
            var dbContextPath = Path.Combine(projectFileInfo.Directory.FullName, "AppContext.cs");

            FileSystem.File.WriteAllText(dbContextPath, dbContextText);
        }

        protected const string ProjectDefinition = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.EntityFrameworkCore.Relational"" Version=""2.2.6"" />
    </ItemGroup>
</Project>";
    }
}
