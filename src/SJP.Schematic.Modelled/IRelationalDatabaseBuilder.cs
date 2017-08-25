using SJP.Schematic.Core;
using System;

namespace SJP.Schematic.Modelled
{
    public interface IRelationalDatabaseBuilder
    {
        IRelationalDatabaseBuilder OverrideWith(IDependentRelationalDatabase database);

        IRelationalDatabaseBuilder OverrideWith(Func<IDependentRelationalDatabase> databaseFactory);

        IRelationalDatabase Build();
    }
}
