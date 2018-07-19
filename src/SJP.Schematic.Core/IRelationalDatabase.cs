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

        IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        bool ViewExists(Identifier viewName);

        IRelationalDatabaseView GetView(Identifier viewName);

        IReadOnlyCollection<IRelationalDatabaseView> Views { get; }

        bool SequenceExists(Identifier sequenceName);

        IDatabaseSequence GetSequence(Identifier sequenceName);

        IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        bool SynonymExists(Identifier synonymName);

        IDatabaseSynonym GetSynonym(Identifier synonymName);

        IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

        // async
        Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
