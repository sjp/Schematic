using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    //TODO: suppport cancellation in every async method?
    // could be as simple as:
    //
    // Task<IEnumerable<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancel = default(CancellationToken));
    //
    // would only make sense for more complicated expression

    // async analogues of every synchronous property/method
    public interface IRelationalDatabaseAsync
    {
        Task<bool> TableExistsAsync(Identifier tableName);

        Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName);

        IObservable<IRelationalDatabaseTable> TablesAsync();

        Task<bool> ViewExistsAsync(Identifier viewName);

        Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName);

        IObservable<IRelationalDatabaseView> ViewsAsync();

        Task<bool> SequenceExistsAsync(Identifier sequenceName);

        Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName);

        IObservable<IDatabaseSequence> SequencesAsync();

        Task<bool> SynonymExistsAsync(Identifier synonymName);

        Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName);

        IObservable<IDatabaseSynonym> SynonymsAsync();

        Task<bool> TriggerExistsAsync(Identifier triggerName);

        Task<IDatabaseTrigger> GetTriggerAsync(Identifier triggerName);

        IObservable<IDatabaseTrigger> TriggersAsync();
    }
}
