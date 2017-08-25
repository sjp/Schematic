using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class Index : IModelledIndex
    {
        public Index(params IndexColumn[] indexColumns)
            : this(indexColumns as IEnumerable<IModelledIndexColumn>) { }

        public Index(params IModelledIndexColumn[] indexColumns)
            : this(indexColumns as IEnumerable<IModelledIndexColumn>) { }

        public Index(IEnumerable<IModelledIndexColumn> indexColumns)
        {
            if (indexColumns == null || indexColumns.Empty() || indexColumns.AnyNull())
                throw new ArgumentNullException(nameof(indexColumns));

            Columns = indexColumns.ToList();
            IncludedColumns = Enumerable.Empty<IModelledColumn>();
        }

        private Index(IModelledIndex index, IEnumerable<IModelledColumn> includedColumns)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));
            if (index.Columns == null || includedColumns.Empty() || includedColumns.AnyNull())
                throw new ArgumentException("The index has no key columns. These are required in order to declare included columns on an index.", nameof(index));
            if (index.IncludedColumns.Any())
                throw new ArgumentException("The index already has included columns on it. Do not declare an index with multiple calls to Include()", nameof(index));
            if (includedColumns == null || includedColumns.Empty() || includedColumns.AnyNull())
                throw new ArgumentNullException(nameof(includedColumns));

            Property = index.Property;
            Columns = index.Columns;
            IsUnique = index.IsUnique;
            IncludedColumns = includedColumns.ToList();
        }

        public Index Include(params IModelledColumn[] includedColumns) => Include(includedColumns as IEnumerable<IModelledColumn>);

        public Index Include(IEnumerable<IModelledColumn> includedColumns) => new Index(this, includedColumns);

        public PropertyInfo Property { get; set; }

        public IEnumerable<IModelledIndexColumn> Columns { get; }

        public IEnumerable<IModelledColumn> IncludedColumns { get; }

        public virtual bool IsUnique { get; }

        public class Unique : Index
        {
            public Unique(params IndexColumn[] indexColumns)
                : this(indexColumns as IEnumerable<IModelledIndexColumn>) { }

            public Unique(IEnumerable<IModelledIndexColumn> indexColumns)
                : base(indexColumns) { }

            public override bool IsUnique { get; } = true;
        }
    }
}
