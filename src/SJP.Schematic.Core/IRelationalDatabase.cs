using System.Collections.Generic;
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
        Task<bool> TableExistsAsync(Identifier tableName);

        Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName);

        Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync();

        Task<bool> ViewExistsAsync(Identifier viewName);

        Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName);

        Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync();

        Task<bool> SequenceExistsAsync(Identifier sequenceName);

        Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName);

        Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync();

        Task<bool> SynonymExistsAsync(Identifier synonymName);

        Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName);

        Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync();
    }
}
