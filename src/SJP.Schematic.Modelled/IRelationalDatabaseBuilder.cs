using SJP.Schema.Core;
using System;

namespace SJP.Schema.Modelled
{
    public interface IRelationalDatabaseBuilder
    {
        IRelationalDatabaseBuilder OverrideWith(IDependentRelationalDatabase database);

        IRelationalDatabaseBuilder OverrideWith(Func<IDependentRelationalDatabase> databaseFactory);

        IRelationalDatabase Build();
    }
}
