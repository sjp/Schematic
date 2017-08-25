using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseCheckConstraint : IDatabaseOptional
    {
        Identifier Name { get; }

        IRelationalDatabaseTable Table { get; }

        string Definition { get; }
    }
}
