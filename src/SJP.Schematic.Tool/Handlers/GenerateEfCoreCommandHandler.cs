using System.CommandLine;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.DataAccess.EntityFrameworkCore;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers
{
    internal sealed class GenerateEfCoreCommandHandler : DatabaseCommandHandler
    {
        public GenerateEfCoreCommandHandler(FileInfo filePath)
            : base(filePath)
        {
        }

        public async Task<int> HandleCommandAsync(IConsole console, FileInfo projectPath, string baseNamespace, NamingConvention convention, CancellationToken cancellationToken)
        {
            var fileSystem = new FileSystem();
            var nameTranslator = GetNameTranslator(convention);
            var connection = GetSchematicConnection();
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);
            var commentProvider = await connection.Dialect.GetRelationalDatabaseCommentProviderAsync(connection, cancellationToken).ConfigureAwait(false);

            var generator = new EFCoreDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);

            await generator.GenerateAsync(projectPath.FullName, baseNamespace, cancellationToken).ConfigureAwait(false);

            console.Out.Write("Project generated at: " + projectPath.FullName);
            return ErrorCode.Success;
        }
    }
}
