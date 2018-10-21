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
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Name = database.Dialect.GetQualifiedNameOrDefault(database, ViewType);
        }

        protected Type ViewType { get; }

        public string Definition => throw new NotImplementedException();

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => throw new NotImplementedException();

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyList<IDatabaseColumn> Columns
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyCollection<IDatabaseIndex> Indexes
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

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
