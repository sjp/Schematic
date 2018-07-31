using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public class RelationalDatabaseView : IRelationalDatabaseView
    {
        public RelationalDatabaseView(
            IRelationalDatabase database,
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseViewColumn> columns,
            IReadOnlyCollection<IDatabaseViewIndex> indexes,
            IEqualityComparer<Identifier> comparer = null)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
            IsIndexed = Indexes.Count > 0;
            Definition = definition;

            var serverName = viewName.Server ?? database.ServerName;
            var databaseName = viewName.Database ?? database.DatabaseName;
            var schemaName = viewName.Schema ?? database.DefaultSchema;

            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, serverName, databaseName, schemaName);

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, viewName.LocalName);

            Column = CreateColumnLookup(Columns, Comparer);
            Index = CreateIndexLookup(Indexes, Comparer);
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public string Definition { get; }

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Definition);

        public bool IsIndexed { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Index);

        public IReadOnlyCollection<IDatabaseViewIndex> Indexes { get; }

        public Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Indexes);

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Column);

        public IReadOnlyList<IDatabaseViewColumn> Columns { get; }

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Columns);

        private static IReadOnlyDictionary<Identifier, IDatabaseViewColumn> CreateColumnLookup(IReadOnlyList<IDatabaseViewColumn> columns, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count, comparer);

            var namedColumns = columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result;
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseViewIndex> CreateIndexLookup(IReadOnlyCollection<IDatabaseViewIndex> indexes, IEqualityComparer<Identifier> comparer)
        {
            var result = new Dictionary<Identifier, IDatabaseViewIndex>(indexes.Count, comparer);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result;
        }
    }
}
