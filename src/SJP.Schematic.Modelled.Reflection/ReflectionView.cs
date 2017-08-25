using System;
using System.Collections.Generic;
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

        public IEnumerable<IDatabaseViewIndex> Indexes
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

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
