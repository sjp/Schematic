using System;
using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.DataAccess.OrmLite;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers
{
    internal sealed class GenerateOrmLiteCommandHandler : DatabaseCommandHandler
    {
        private readonly IConsole _console;

        public GenerateOrmLiteCommandHandler(IConsole console, FileInfo filePath)
            : base(filePath)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public async Task<int> HandleCommandAsync(FileInfo projectPath, string baseNamespace, NamingConvention convention, CancellationToken cancellationToken)
        {
            var fileSystem = new FileSystem();
            var nameTranslator = GetNameTranslator(convention);
            var connection = GetSchematicConnection();
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);
            var commentProvider = await connection.Dialect.GetRelationalDatabaseCommentProviderAsync(connection, cancellationToken).ConfigureAwait(false);

            var generator = new OrmLiteDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);

            await generator.GenerateAsync(projectPath.FullName, baseNamespace, cancellationToken).ConfigureAwait(false);

            _console.Out.Write("Project generated at: " + projectPath.FullName);
            return ErrorCode.Success;
        }
    }
}
