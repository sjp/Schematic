using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledSqlExpression
    {
        string ToSql(IDatabaseDialect dialect);

        IEnumerable<Identifier> DependentNames { get; }

        bool IsIdentity { get; }
    }
}
