using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionCheckConstraint : IDatabaseCheckConstraint
    {
        public ReflectionCheckConstraint(IRelationalDatabaseTable table, Identifier name, ISqlExpression expression, IEnumerable<IDatabaseColumn> columns)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            DependentColumns = columns;
        }

        public IEnumerable<IDatabaseColumn> DependentColumns { get; }

        public ISqlExpression Expression { get; }

        public Identifier Name { get; }

        public IRelationalDatabaseTable Table { get; }

        // this should always be true
        // is there a situation where would not want it to be true?
        public bool IsEnabled { get; } = true;
    }
}
