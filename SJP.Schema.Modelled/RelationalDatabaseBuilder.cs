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

            _databases.Add(database);
        }

        protected RelationalDatabaseBuilder(IList<IDependentRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Count == 0)
                throw new ArgumentException("At least one database must be provided.", nameof(databases));

            _databases = databases;
        }

        public RelationalDatabaseBuilder(Func<IDependentRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            _databases.Add(result);
        }

        public IRelationalDatabaseBuilder OverrideWith(IDependentRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            _databases.Add(database);
            return new RelationalDatabaseBuilder(_databases);
        }

        public IRelationalDatabaseBuilder OverrideWith(Func<IDependentRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            _databases.Add(result);

            return new RelationalDatabaseBuilder(_databases);
        }

        public IRelationalDatabase Build() => new OrderedRelationalDatabase(_databases.Reverse());

        private readonly IList<IDependentRelationalDatabase> _databases = new List<IDependentRelationalDatabase>();
    }
}
