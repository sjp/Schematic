using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled
{
    public class RelationalDatabaseBuilder : IRelationalDatabaseBuilder
    {
        public RelationalDatabaseBuilder(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            _databases = new List<IRelationalDatabase> { database };
        }

        public RelationalDatabaseBuilder(Func<IRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            _databases = new List<IRelationalDatabase> { result };
        }

        protected RelationalDatabaseBuilder(IEnumerable<IRelationalDatabase> databases)
        {
            if (databases == null)
                throw new ArgumentNullException(nameof(databases));
            if (databases.Empty())
                throw new ArgumentException("At least one database must be provided.", nameof(databases));

            _databases = databases.ToList();
        }

        public IRelationalDatabaseBuilder OverrideWith(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var appendedDatabases = new List<IRelationalDatabase>(_databases) { database };
            return new RelationalDatabaseBuilder(appendedDatabases);
        }

        public IRelationalDatabaseBuilder OverrideWith(Func<IRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            var appendedDatabases = new List<IRelationalDatabase>(_databases) { result };
            return new RelationalDatabaseBuilder(appendedDatabases);
        }

        public IRelationalDatabase Build() => new OrderedRelationalDatabase(_databases.Reverse());

        private readonly IList<IRelationalDatabase> _databases;
    }
}
