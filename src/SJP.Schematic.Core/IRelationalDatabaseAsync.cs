using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseAsync
    {
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
