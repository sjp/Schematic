using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests.Fakes
{
    internal class FakeRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IDatabaseIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
        }

        public virtual IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; set; } = new List<IRelationalDatabaseTable>();

        public virtual IReadOnlyCollection<IRelationalDatabaseView> Views { get; set; } = new List<IRelationalDatabaseView>();

        public virtual IReadOnlyCollection<IDatabaseSequence> Sequences { get; set; } = new List<IDatabaseSequence>();

        public virtual IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; set; } = new List<IDatabaseSynonym>();

        public virtual Option<IDatabaseSequence> GetSequence(Identifier sequenceName) => null;

        public virtual OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IDatabaseSequence>.None;

        public virtual Option<IDatabaseSynonym> GetSynonym(Identifier synonymName) => null;

        public virtual OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IDatabaseSynonym>.None;

        public virtual Option<IRelationalDatabaseTable> GetTable(Identifier tableName) => null;

        public virtual OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IRelationalDatabaseTable>.None;

        public virtual Option<IRelationalDatabaseView> GetView(Identifier viewName) => null;

        public virtual OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IRelationalDatabaseView>.None;

        public virtual Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Sequences);

        public virtual Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Synonyms);

        public virtual Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Tables);

        public virtual Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Views);
    }
}
