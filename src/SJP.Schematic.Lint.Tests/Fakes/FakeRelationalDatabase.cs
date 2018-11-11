using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests.Fakes
{
    internal class FakeRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
        }

        public virtual string ServerName { get; set; }

        public virtual string DatabaseName { get; set; }

        public virtual string DefaultSchema { get; set; }

        public virtual string DatabaseVersion { get; set; }

        public virtual IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; set; } = new List<IRelationalDatabaseTable>();

        public virtual IReadOnlyCollection<IRelationalDatabaseView> Views { get; set; } = new List<IRelationalDatabaseView>();

        public virtual IReadOnlyCollection<IDatabaseSequence> Sequences { get; set; } = new List<IDatabaseSequence>();

        public virtual IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; set; } = new List<IDatabaseSynonym>();

        public virtual Option<IDatabaseSequence> GetSequence(Identifier sequenceName) => null;

        public virtual Task<Option<IDatabaseSequence>> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Option<IDatabaseSequence>.None);

        public virtual Option<IDatabaseSynonym> GetSynonym(Identifier synonymName) => null;

        public virtual Task<Option<IDatabaseSynonym>> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Option<IDatabaseSynonym>.None);

        public virtual Option<IRelationalDatabaseTable> GetTable(Identifier tableName) => null;

        public virtual Task<Option<IRelationalDatabaseTable>> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Option<IRelationalDatabaseTable>.None);

        public virtual Option<IRelationalDatabaseView> GetView(Identifier viewName) => null;

        public virtual Task<Option<IRelationalDatabaseView>> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Option<IRelationalDatabaseView>.None);

        public virtual Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IDatabaseSequence>>>(
            Sequences.Select(Task.FromResult).ToList()
        );

        public virtual Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IDatabaseSynonym>>>(
            Synonyms.Select(Task.FromResult).ToList()
        );

        public virtual Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IRelationalDatabaseTable>>>(
            Tables.Select(Task.FromResult).ToList()
        );

        public virtual Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult<IReadOnlyCollection<Task<IRelationalDatabaseView>>>(
            Views.Select(Task.FromResult).ToList()
        );
    }
}
