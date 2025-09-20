using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Tests.Fakes;

internal class FakeRelationalDatabase : IRelationalDatabase
{
    public FakeRelationalDatabase(IIdentifierDefaults identifierDefaults)
    {
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    public IIdentifierDefaults IdentifierDefaults { get; }

    public IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; set; } = [];

    public IReadOnlyCollection<IDatabaseView> Views { get; set; } = [];

    public IReadOnlyCollection<IDatabaseSequence> Sequences { get; set; } = [];

    public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; set; } = [];

    public IReadOnlyCollection<IDatabaseRoutine> Routines { get; set; } = [];

    public virtual OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        return Sequences.Find(s => s.Name == sequenceName).ToAsync();
    }

    public virtual OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        return Synonyms.Find(s => s.Name == synonymName).ToAsync();
    }

    public virtual OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        return Tables.Find(t => t.Name == tableName).ToAsync();
    }

    public virtual OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        return Views.Find(v => v.Name == viewName).ToAsync();
    }

    public virtual OptionAsync<IDatabaseRoutine> GetRoutine(Identifier routineName, CancellationToken cancellationToken = default)
    {
        return Routines.Find(r => r.Name == routineName).ToAsync();
    }

    public virtual IAsyncEnumerable<IDatabaseSequence> EnumerateAllSequences(CancellationToken cancellationToken = default) => Sequences.ToAsyncEnumerable();

    public virtual IAsyncEnumerable<IDatabaseSynonym> EnumerateAllSynonyms(CancellationToken cancellationToken = default) => Synonyms.ToAsyncEnumerable();

    public virtual IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default) => Tables.ToAsyncEnumerable();

    public virtual IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default) => Views.ToAsyncEnumerable();

    public virtual IAsyncEnumerable<IDatabaseRoutine> EnumerateAllRoutines(CancellationToken cancellationToken = default) => Routines.ToAsyncEnumerable();

    public virtual Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences2(CancellationToken cancellationToken = default) => Task.FromResult(Sequences);

    public virtual Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms2(CancellationToken cancellationToken = default) => Task.FromResult(Synonyms);

    public virtual Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables2(CancellationToken cancellationToken = default) => Task.FromResult(Tables);

    public virtual Task<IReadOnlyCollection<IDatabaseView>> GetAllViews2(CancellationToken cancellationToken = default) => Task.FromResult(Views);

    public virtual Task<IReadOnlyCollection<IDatabaseRoutine>> GetAllRoutines2(CancellationToken cancellationToken = default) => Task.FromResult(Routines);
}