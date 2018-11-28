using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabase
    {
        // sync
        IDatabaseDialect Dialect { get; }

        string DatabaseVersion { get; }

        string ServerName { get; }

        string DatabaseName { get; }

        string DefaultSchema { get; }

        Option<IRelationalDatabaseTable> GetTable(Identifier tableName);

        IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

        Option<IRelationalDatabaseView> GetView(Identifier viewName);

        IReadOnlyCollection<IRelationalDatabaseView> Views { get; }

        Option<IDatabaseSequence> GetSequence(Identifier sequenceName);

        IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        Option<IDatabaseSynonym> GetSynonym(Identifier synonymName);

        IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

        // async
        OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken));

        OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken));

        OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken));

        OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
