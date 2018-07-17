using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase
    {
        // sync
        IDatabaseDialect Dialect { get; }

        string ServerName { get; }

        string DatabaseName { get; }

        string DefaultSchema { get; }

        bool TableExists(Identifier tableName);

        IRelationalDatabaseTable GetTable(Identifier tableName);

        IEnumerable<IRelationalDatabaseTable> Tables { get; }

        bool ViewExists(Identifier viewName);

        IRelationalDatabaseView GetView(Identifier viewName);

        IEnumerable<IRelationalDatabaseView> Views { get; }

        bool SequenceExists(Identifier sequenceName);

        IDatabaseSequence GetSequence(Identifier sequenceName);

        IEnumerable<IDatabaseSequence> Sequences { get; }

        bool SynonymExists(Identifier synonymName);

        IDatabaseSynonym GetSynonym(Identifier synonymName);

        IEnumerable<IDatabaseSynonym> Synonyms { get; }

        // async
        Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
