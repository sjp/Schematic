using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schema.Core
{
    public class RelationalDatabaseBuilder : IRelationalDatabaseBuilder
    {
        public RelationalDatabaseBuilder(IDependentRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            _databases.Add(database);
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

            return this;
        }

        public IRelationalDatabaseBuilder OverrideWith(Func<IDependentRelationalDatabase> databaseFactory)
        {
            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));

            var result = databaseFactory.Invoke();
            _databases.Add(result);

            return this;
        }

        public IRelationalDatabase Build() => new OrderedRelationalDatabase(_databases.Reverse());

        private readonly IList<IDependentRelationalDatabase> _databases = new List<IDependentRelationalDatabase>();
    }
}
