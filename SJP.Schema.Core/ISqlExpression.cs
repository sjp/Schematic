using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.Schema.Core
{
    public interface ISqlExpression
    {
        string ToSql(IDatabaseDialect dialect);

        IEnumerable<Identifier> DependentNames { get; }

        bool IsIdentity { get; }
    }
}
