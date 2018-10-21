using SJP.Schematic.Core;
using System;

namespace SJP.Schematic.Modelled
{
    public interface IRelationalDatabaseBuilder
    {
        IRelationalDatabaseBuilder OverrideWith(IRelationalDatabase database);

        IRelationalDatabaseBuilder OverrideWith(Func<IRelationalDatabase> databaseFactory);

        IRelationalDatabase Build();
    }
}
