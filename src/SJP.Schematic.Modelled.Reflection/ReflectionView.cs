using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionView : IRelationalDatabaseView
    {
        public ReflectionView(IRelationalDatabase database, Type viewType)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Name = Database.Dialect.GetQualifiedNameOrDefault(Database, ViewType);
        }

        protected Type ViewType { get; }

        public string Definition => throw new NotImplementedException();

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => throw new NotImplementedException();

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyList<IDatabaseViewColumn> Columns
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IRelationalDatabase Database { get; }

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyCollection<IDatabaseViewIndex> Indexes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsIndexed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Identifier Name { get; }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
