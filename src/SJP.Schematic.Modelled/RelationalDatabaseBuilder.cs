using SJP.Schema.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schema.Modelled
{
    public class RelationalDatabaseBuilder : IRelationalDatabaseBuilder
    {
        public RelationalDatabaseBuilder(IDependentRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            _databases = new List<IDependentRelationalDatabase> { database };
        }

        public RelationalDatabaseBuilder(Func<IDependentRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            _databases = new List<IDependentRelationalDatabase> { result };
        }

        protected RelationalDatabaseBuilder(IEnumerable<IDependentRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Empty())
                throw new ArgumentException("At least one database must be provided.", nameof(databases));

            _databases = databases.ToList();
        }

        public IRelationalDatabaseBuilder OverrideWith(IDependentRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var appendedDatabases = new List<IDependentRelationalDatabase>(_databases) { database };
            return new RelationalDatabaseBuilder(_databases);
        }

        public IRelationalDatabaseBuilder OverrideWith(Func<IDependentRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            var appendedDatabases = new List<IDependentRelationalDatabase>(_databases) { result };
            return new RelationalDatabaseBuilder(appendedDatabases);
        }

        public IRelationalDatabase Build() => new OrderedRelationalDatabase(_databases.Reverse());

        private readonly IList<IDependentRelationalDatabase> _databases;
    }
}
