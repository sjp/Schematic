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
        public FakeRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierDefaults identifierDefaults)
            : base(dialect, connection, identifierDefaults)
        {
        }

        public virtual IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; set; } = new List<IRelationalDatabaseTable>();

        public virtual IReadOnlyCollection<IRelationalDatabaseView> Views { get; set; } = new List<IRelationalDatabaseView>();

        public virtual IReadOnlyCollection<IDatabaseSequence> Sequences { get; set; } = new List<IDatabaseSequence>();

        public virtual IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; set; } = new List<IDatabaseSynonym>();

        public virtual OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IDatabaseSequence>.None;

        public virtual OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IDatabaseSynonym>.None;

        public virtual OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IRelationalDatabaseTable>.None;

        public virtual OptionAsync<IRelationalDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken)) => OptionAsync<IRelationalDatabaseView>.None;

        public virtual Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Sequences);

        public virtual Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Synonyms);

        public virtual Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Tables);

        public virtual Task<IReadOnlyCollection<IRelationalDatabaseView>> GetAllViews(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Views);
    }
}
