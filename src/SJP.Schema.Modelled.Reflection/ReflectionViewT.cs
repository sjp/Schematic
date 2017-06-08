using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionView<T> : IRelationalDatabaseView where T : class, new()
    {
        public ReflectionView(IRelationalDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = Database.Dialect.GetQualifiedNameOverrideOrDefault(Database, ViewType);
        }

        public IReadOnlyDictionary<string, IDatabaseViewColumn> Column
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IList<IDatabaseViewColumn> Columns
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IRelationalDatabase Database { get; }

        public IEnumerable<Identifier> Dependencies
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<Identifier> Dependents
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyDictionary<string, IDatabaseViewIndex> Index
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

        protected Type ViewType { get; } = typeof(T);

        public Task<IReadOnlyDictionary<string, IDatabaseViewColumn>> ColumnAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IList<IDatabaseViewColumn>> ColumnsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Identifier>> DependenciesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Identifier>> DependentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<string, IDatabaseViewIndex>> IndexAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
