using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionDatabaseTableIndex : IDatabaseTableIndex
    {
        public ReflectionDatabaseTableIndex(IRelationalDatabaseTable table, IModelledIndex index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            Parent = table ?? throw new ArgumentNullException(nameof(table));
            IsUnique = index.IsUnique;

            var dialect = table.Database.Dialect;
            Name = dialect.GetAliasOrDefault(index.Property);

            // TODO:
            // fix column lookup to work on property infos instead of string names, probably easier to map
            // and gives us dialect info later

            var columns = new List<IDatabaseIndexColumn>();
            var includedColumns = new List<IDatabaseTableColumn>();

            var isFunctionBasedIndex = false;

            foreach (var indexColumn in index.Columns)
            {
                if (indexColumn.Expression.IsIdentity)
                {
                    var tableColumns = new List<IDatabaseColumn> { Parent.Column[indexColumn.Expression.DependentNames.Single().LocalName] };
                    var column = new ReflectionIndexColumn(indexColumn.Expression, tableColumns, indexColumn.Order);
                    columns.Add(column);
                }
                else
                {
                    isFunctionBasedIndex = true;
                    var tableColumns = indexColumn.Expression.DependentNames
                        .Where(name => Parent.Column.ContainsKey(name.LocalName))
                        .Select(name => table.Column[name.LocalName] as IDatabaseColumn)
                        .ToList();
                    var column = new ReflectionIndexColumn(indexColumn.Expression, tableColumns, indexColumn.Order);
                    columns.Add(column);
                }
            }

            foreach (var includedColumn in index.IncludedColumns)
            {
                var includedColumnName = dialect.GetAliasOrDefault(includedColumn.Property);
                var column = table.Column[includedColumnName];
                includedColumns.Add(column);
            }

            IsFunctionBased = isFunctionBasedIndex;

            Columns = columns.ToImmutableList();
            IncludedColumns = includedColumns.ToImmutableList();
        }

        public IRelationalDatabaseTable Table { get; }

        public IRelationalDatabaseTable Parent { get; }

        public IEnumerable<IDatabaseIndexColumn> Columns { get; }

        public IEnumerable<IDatabaseColumn> IncludedColumns { get; }

        public bool IsUnique { get; }

        public Identifier Name { get; }

        public bool IsFunctionBased { get; }
    }
}
