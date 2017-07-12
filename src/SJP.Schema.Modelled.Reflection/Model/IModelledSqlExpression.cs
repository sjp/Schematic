using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledSqlExpression
    {
        string ToSql(IDatabaseDialect dialect);

        IEnumerable<Identifier> DependentNames { get; }

        bool IsIdentity { get; }
    }
}
