using System;

namespace SJP.Schema.Core
{
    public interface IRelationalDatabaseBuilder
    {
        IRelationalDatabaseBuilder OverrideWith(IDependentRelationalDatabase database);

        IRelationalDatabaseBuilder OverrideWith(Func<IDependentRelationalDatabase> databaseFactory);

        IRelationalDatabase Build();
    }
}
